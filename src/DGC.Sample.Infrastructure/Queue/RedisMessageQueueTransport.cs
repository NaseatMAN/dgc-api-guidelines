using System.Text.Json;
using DGC.Sample.Application.Queue;
using DGC.Sample.Application.Queue.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace DGC.Sample.Infrastructure.Queue;

public sealed class RedisMessageQueueTransport<T>(IConnectionMultiplexer multiplexer, IConfiguration configuration) : IMessageQueueTransport<T>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private readonly IDatabase _database = multiplexer.GetDatabase();
    private readonly int _maxPayloadBytes = Math.Max(1024, configuration.GetValue<int?>("Queue:MaxPayloadBytes") ?? 262_144);
    private readonly bool _deadLetterEnabled = configuration.GetValue<bool?>("Queue:DeadLetter:Enabled") ?? true;
    private readonly string _deadLetterPrefix = configuration.GetValue<string>("Queue:DeadLetter:Prefix") ?? "dlq";

    public QueueTransport TransportType => QueueTransport.Redis;

    public async Task EnqueueAsync(T item, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        var envelope = CreateEnvelope(item);
        var payload = SerializeEnvelope(envelope);

        if (payload.Length > _maxPayloadBytes)
        {
            throw new QueueProcessingException(
                $"Queue payload exceeds max allowed bytes ({_maxPayloadBytes}).",
                new InvalidOperationException("Payload too large"));
        }

        var queueKey = GetQueueKey();
        await _database.ListLeftPushAsync(queueKey, payload).ConfigureAwait(false);
    }

    public async Task<Envelope<T>?> DequeueAsync(int waitMs, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        var queueKey = GetQueueKey();
        var processingKey = GetProcessingListKey();

        RedisValue payload;
        if (waitMs <= 0)
        {
            payload = await _database.ListRightPopLeftPushAsync(queueKey, processingKey).ConfigureAwait(false);
        }
        else
        {
            var timeoutSeconds = (int)Math.Ceiling(waitMs / 1000d);
            var result = await _database.ExecuteAsync("BRPOPLPUSH", queueKey, processingKey, timeoutSeconds).ConfigureAwait(false);
            payload = result.IsNull ? RedisValue.Null : (string?)result;
        }

        if (payload.IsNullOrEmpty)
        {
            return null;
        }

        var envelope = DeserializeEnvelope(payload!);
        if (envelope is null)
        {
            return null;
        }

        var processingMapKey = GetProcessingMapKey();
        await _database.HashSetAsync(processingMapKey, envelope.Id, payload).ConfigureAwait(false);

        return envelope;
    }

    public async Task AcknowledgeAsync(string envelopeId, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        var processingMapKey = GetProcessingMapKey();
        var serialized = await _database.HashGetAsync(processingMapKey, envelopeId).ConfigureAwait(false);
        if (!serialized.IsNullOrEmpty)
        {
            await _database.ListRemoveAsync(GetProcessingListKey(), serialized, 1).ConfigureAwait(false);
        }

        await _database.HashDeleteAsync(processingMapKey, envelopeId).ConfigureAwait(false);
    }

    public async Task HandleProcessingErrorAsync(
        Envelope<T> envelope,
        int retryLimit,
        int retryDelayMs,
        ILogger logger,
        Exception exception,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        envelope.DeliveryCount++;
        envelope.LastAttemptAt = DateTimeOffset.UtcNow;
        envelope.LastError = exception.Message;

        var processingMapKey = GetProcessingMapKey();
        var originalSerialized = await _database.HashGetAsync(processingMapKey, envelope.Id).ConfigureAwait(false);

        if (envelope.DeliveryCount > retryLimit)
        {
            if (_deadLetterEnabled)
            {
                var deadLetterKey = GetDeadLetterKey();
                var deadLetterPayload = SerializeEnvelope(envelope);
                await _database.ListLeftPushAsync(deadLetterKey, deadLetterPayload).ConfigureAwait(false);
            }

            if (!originalSerialized.IsNullOrEmpty)
            {
                await _database.ListRemoveAsync(GetProcessingListKey(), originalSerialized, 1).ConfigureAwait(false);
            }

            await _database.HashDeleteAsync(processingMapKey, envelope.Id).ConfigureAwait(false);

            logger.LogError(
                exception,
                "Message moved to Redis dead-letter queue. envelopeId={EnvelopeId} messageType={MessageType}",
                envelope.Id,
                typeof(T).Name);
            return;
        }

        logger.LogWarning(
            exception,
            "Retrying Redis message. envelopeId={EnvelopeId} attempt={Attempt}",
            envelope.Id,
            envelope.DeliveryCount);

        if (retryDelayMs > 0)
        {
            await Task.Delay(retryDelayMs, token).ConfigureAwait(false);
        }

        var updatedSerialized = SerializeEnvelope(envelope);

        if (!originalSerialized.IsNullOrEmpty)
        {
            await _database.ListRemoveAsync(GetProcessingListKey(), originalSerialized, 1).ConfigureAwait(false);
        }

        await _database.HashDeleteAsync(processingMapKey, envelope.Id).ConfigureAwait(false);
        await _database.ListLeftPushAsync(GetQueueKey(), updatedSerialized).ConfigureAwait(false);
    }

    private static Envelope<T>? DeserializeEnvelope(string payload)
    {
        try
        {
            return JsonSerializer.Deserialize<Envelope<T>>(payload, JsonOptions);
        }
        catch (JsonException ex)
        {
            throw new QueueProcessingException("Failed to deserialize queue envelope.", ex);
        }
    }

    private static string SerializeEnvelope(Envelope<T> envelope)
    {
        return JsonSerializer.Serialize(envelope, JsonOptions);
    }

    private static Envelope<T> CreateEnvelope(T item)
    {
        return new Envelope<T>(
            id: Guid.NewGuid().ToString("N"),
            payload: item,
            deliveryCount: 0,
            enqueuedAt: DateTimeOffset.UtcNow,
            typeName: typeof(T).FullName ?? typeof(T).Name,
            schemaVersion: 1);
    }

    private static string GetQueueName()
    {
        var messageName = typeof(T).Name.ToLowerInvariant();
        return $"queue:{messageName}";
    }

    private string GetQueueKey() => GetQueueName();

    private string GetProcessingListKey() => $"{GetQueueName()}:processing";

    private string GetProcessingMapKey() => $"{GetQueueName()}:processing-map";

    private string GetDeadLetterKey() => $"{_deadLetterPrefix}:{GetQueueName()}";
}