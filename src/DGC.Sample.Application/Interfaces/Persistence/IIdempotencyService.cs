using DGC.Sample.Application.Dtos;

namespace DGC.Sample.Application.Interfaces.Persistence;

public interface IIdempotencyService
{
    Task<IdempotencyExecutionResult> TryStartRequestAsync(string idempotencyKey, string requestHash, CancellationToken cancellationToken);
    Task SaveRequestAsync(
        string idempotencyKey,
        string requestHash,
        int statusCode,
        string responseBody,
        string contentType,
        CancellationToken cancellationToken);
    Task ReleaseRequestAsync(string idempotencyKey, string requestHash, CancellationToken cancellationToken);
}
