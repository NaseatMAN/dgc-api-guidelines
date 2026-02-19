namespace DGC.Sample.Application.Dtos;

public sealed record IdempotencyResult(int StatusCode, string ResponseBody, bool IsProcessing = false);
