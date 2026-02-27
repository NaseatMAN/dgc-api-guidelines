namespace DGC.Sample.Application.Dtos.Queue;

public sealed record Envelope<T>
{
    public Envelope()
    {
    }

    public Envelope(
        string id,
        T payload,
        int deliveryCount,
        DateTimeOffset enqueuedAt,
        string typeName,
        int schemaVersion,
        string? correlationId = null,
        string? causationId = null,
        DateTimeOffset? lastAttemptAt = null)
    {
        Id = id;
        Payload = payload;
        DeliveryCount = deliveryCount;
        EnqueuedAt = enqueuedAt;
        TypeName = typeName;
        SchemaVersion = schemaVersion;
        CorrelationId = correlationId;
        CausationId = causationId;
        LastAttemptAt = lastAttemptAt;
    }

    public string Id { get; set; } = string.Empty;

    public T Payload { get; set; } = default!;

    public int DeliveryCount { get; set; }

    public DateTimeOffset EnqueuedAt { get; set; }

    public string TypeName { get; set; } = string.Empty;

    public int SchemaVersion { get; set; } = 1;

    public string? CorrelationId { get; set; }

    public string? CausationId { get; set; }

    public DateTimeOffset? LastAttemptAt { get; set; }

    public string? LastError { get; set; }
}