using DGC.Sample.Domain.Entities;
using DGC.Sample.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace DGC.Sample.Infrastructure.Persistence;

public interface IIdempotencyService
{
    Task<IdempotentRequest?> GetRequestAsync(string idempotencyKey, CancellationToken cancellationToken);
    Task SaveRequestAsync(string idempotencyKey, string path, int statusCode, string responseBody, CancellationToken cancellationToken);
}

public sealed class IdempotencyService(AppDbContext dbContext) : IIdempotencyService
{
    private readonly AppDbContext _dbContext = dbContext;

    public Task<IdempotentRequest?> GetRequestAsync(string idempotencyKey, CancellationToken cancellationToken)
    {
        return _dbContext.IdempotentRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.IdempotencyKey == idempotencyKey, cancellationToken);
    }

    public async Task SaveRequestAsync(string idempotencyKey, string path, int statusCode, string responseBody, CancellationToken cancellationToken)
    {
        var request = new IdempotentRequest
        {
            IdempotencyKey = idempotencyKey,
            RequestPath = path,
            StatusCode = statusCode,
            ResponseBody = responseBody
        };

        _dbContext.IdempotentRequests.Add(request);
        
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Another request might have saved it already in a race condition
            // We can ignore this as the GetRequest will handle it in the next retry or it was already handled.
        }
    }
}
