namespace DGC.Sample.Application.Dtos;

public enum IdempotencyExecutionState
{
    Started,
    Completed,
    Processing,
    RequestMismatch
}

public sealed record IdempotencyResult(
    int StatusCode,
    string ResponseBody,
    string RequestHash,
    string ContentType = "application/json",
    bool IsProcessing = false);

public sealed record IdempotencyExecutionResult(
    IdempotencyExecutionState State,
    IdempotencyResult? CachedResponse = null);
