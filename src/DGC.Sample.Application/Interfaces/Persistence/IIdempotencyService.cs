using DGC.Sample.Application.Dtos;

namespace DGC.Sample.Application.Interfaces.Persistence;

    public interface IIdempotencyService
{
    Task<IdempotencyResult?> GetRequestAsync(string idempotencyKey, CancellationToken cancellationToken);
    Task<bool> TryStartRequestAsync(string idempotencyKey, CancellationToken cancellationToken);
    Task SaveRequestAsync(string idempotencyKey, int statusCode, string responseBody, CancellationToken cancellationToken);
}
