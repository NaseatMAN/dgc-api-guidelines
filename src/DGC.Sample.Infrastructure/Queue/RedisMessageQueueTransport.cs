using System.Text.Json;
using DGC.Sample.Application.Common.Queue;
using DGC.Sample.Application.Dtos.Queue;
using DGC.Sample.Application.Interfaces.Queue;
using DGC.Sample.Domain.Exceptions;
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

    public async Task EnqueueAsync(T item, CancellationToken token)
    {
        await EnqueueAsync(item, queueName: null, token).ConfigureAwait(false);
    }

    public async Task EnqueueAsync(T item, string? queueName, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var envelope = CreateEnvelope(item);
        var payload = SerializeEnvelope(envelope);

        if (payload.Length > _maxPayloadBytes)
        {
            throw new InternalErrorException(
                InternalErrorCode.QueueProcessingError,
                $"Queue payload exceeds max allowed bytes ({_maxPayloadBytes}).",
                innerCode: "payload_too_large",
                innerMessage: "Queue payload size is larger than configured maximum.");
        }

        var queueKey = GetQueueKey(queueName);
        await _database.ListLeftPushAsync(queueKey, payload).ConfigureAwait(false);
    }

    public async Task<Envelope<T>?> DequeueAsync(int waitMs, CancellationToken token)
    {
        return await DequeueAsync(waitMs, queueName: null, token).ConfigureAwait(false);
    }

    public async Task<Envelope<T>?> DequeueAsync(int waitMs, string? queueName, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var normalizedQueueName = NormalizeQueueName(queueName);
        var queueKey = GetQueueKey(normalizedQueueName);
        var processingKey = GetProcessingListKey(normalizedQueueName);

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

        var processingMapKey = GetProcessingMapKey(normalizedQueueName);
        await _database.HashSetAsync(processingMapKey, envelope.Id, payload).ConfigureAwait(false);
        await _database.HashSetAsync(GetRoutingMapKey(), envelope.Id, normalizedQueueName ?? string.Empty).ConfigureAwait(false);

        return envelope;
    }

    public async Task AcknowledgeAsync(string envelopeId, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var queueName = await GetQueueNameByEnvelopeIdAsync(envelopeId).ConfigureAwait(false);
        var processingMapKey = GetProcessingMapKey(queueName);
        var serialized = await _database.HashGetAsync(processingMapKey, envelopeId).ConfigureAwait(false);
        if (!serialized.IsNullOrEmpty)
        {
            await _database.ListRemoveAsync(GetProcessingListKey(queueName), serialized, 1).ConfigureAwait(false);
        }

        await _database.HashDeleteAsync(processingMapKey, envelopeId).ConfigureAwait(false);
        await _database.HashDeleteAsync(GetRoutingMapKey(), envelopeId).ConfigureAwait(false);
    }

    public async Task HandleProcessingErrorAsync(
        Envelope<T> envelope,
        int retryLimit,
        int retryDelayMs,
        ILogger logger,
        Exception exception,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        envelope.DeliveryCount++;
        envelope.LastAttemptAt = DateTimeOffset.UtcNow;
        envelope.LastError = exception.Message;

        var queueName = await GetQueueNameByEnvelopeIdAsync(envelope.Id).ConfigureAwait(false);
        var processingMapKey = GetProcessingMapKey(queueName);
        var originalSerialized = await _database.HashGetAsync(processingMapKey, envelope.Id).ConfigureAwait(false);

        if (envelope.DeliveryCount > retryLimit)
        {
            if (_deadLetterEnabled)
            {
                var deadLetterKey = GetDeadLetterKey();
                if (queueName is not null)
                {
                    deadLetterKey = GetDeadLetterKey(queueName);
                }
                var deadLetterPayload = SerializeEnvelope(envelope);
                await _database.ListLeftPushAsync(deadLetterKey, deadLetterPayload).ConfigureAwait(false);
            }

            if (!originalSerialized.IsNullOrEmpty)
            {
                await _database.ListRemoveAsync(GetProcessingListKey(queueName), originalSerialized, 1).ConfigureAwait(false);
            }

            await _database.HashDeleteAsync(processingMapKey, envelope.Id).ConfigureAwait(false);
            await _database.HashDeleteAsync(GetRoutingMapKey(), envelope.Id).ConfigureAwait(false);

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
            await _database.ListRemoveAsync(GetProcessingListKey(queueName), originalSerialized, 1).ConfigureAwait(false);
        }

        await _database.HashDeleteAsync(processingMapKey, envelope.Id).ConfigureAwait(false);
        await _database.ListLeftPushAsync(GetQueueKey(queueName), updatedSerialized).ConfigureAwait(false);
    }

    private static Envelope<T>? DeserializeEnvelope(string payload)
    {
        try
        {
            return JsonSerializer.Deserialize<Envelope<T>>(payload, JsonOptions);
        }
        catch (JsonException ex)
        {
            throw new InternalErrorException(
                InternalErrorCode.QueueProcessingError,
                "Failed to deserialize queue envelope.",
                innerCode: ex.GetType().Name,
                innerMessage: ex.Message);
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

    private string GetQueueKey(string? queueName = null) => string.IsNullOrWhiteSpace(queueName)
        ? GetQueueName()
        : $"queue:{queueName.Trim().ToLowerInvariant()}:{typeof(T).Name.ToLowerInvariant()}";

    private string GetProcessingListKey(string? queueName = null) => $"{GetQueueKey(queueName)}:processing";

    private string GetProcessingMapKey(string? queueName = null) => $"{GetQueueKey(queueName)}:processing-map";

    private string GetRoutingMapKey() => $"queue:{typeof(T).Name.ToLowerInvariant()}:processing-routing";

    private string GetDeadLetterKey(string? queueName = null) => $"{_deadLetterPrefix}:{GetQueueKey(queueName)}";

    private static string? NormalizeQueueName(string? queueName)
    {
        return string.IsNullOrWhiteSpace(queueName)
            ? null
            : queueName.Trim().ToLowerInvariant();
    }

    private async Task<string?> GetQueueNameByEnvelopeIdAsync(string envelopeId)
    {
        var storedValue = await _database.HashGetAsync(GetRoutingMapKey(), envelopeId).ConfigureAwait(false);
        if (storedValue.IsNull)
        {
            return null;
        }

        var normalized = storedValue.ToString();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }
}