namespace DGC.Sample.Domain.Entities;

public sealed class IdempotentRequest
{
    public required string IdempotencyKey { get; set; }
    public required string RequestPath { get; set; }
    public required int StatusCode { get; set; }
    public required string ResponseBody { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
