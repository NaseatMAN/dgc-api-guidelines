using System.Collections.Concurrent;
using DGC.Sample.Application.Common.Queue;
using DGC.Sample.Application.Dtos.Queue;
using DGC.Sample.Application.Interfaces.Queue;
using Microsoft.Extensions.Logging;

namespace DGC.Sample.Infrastructure.Queue;

public sealed class InMemoryMessageQueueTransport<T> : IMessageQueueTransport<T>
{
    private readonly ConcurrentDictionary<string, QueueState> _queues = new();
    private readonly ConcurrentDictionary<string, (string QueueKey, Envelope<T> Envelope)> _inflight = new();

    public QueueTransport TransportType => QueueTransport.InMemory;

    public Task EnqueueAsync(T item, CancellationToken token = default)
    {
        return EnqueueAsync(item, queueName: null, token);
    }

    public Task EnqueueAsync(T item, string? queueName, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        var queueKey = GetQueueKey(queueName);
        var queueState = GetQueueState(queueKey);

        var envelope = CreateEnvelope(item);
        queueState.Queue.Enqueue(envelope);
        queueState.Signal.Release();

        return Task.CompletedTask;
    }

    public async Task<Envelope<T>?> DequeueAsync(int waitMs, CancellationToken token = default)
    {
        return await DequeueAsync(waitMs, queueName: null, token).ConfigureAwait(false);
    }

    public async Task<Envelope<T>?> DequeueAsync(int waitMs, string? queueName, CancellationToken token = default)
    {
        var queueKey = GetQueueKey(queueName);
        var queueState = GetQueueState(queueKey);

        if (queueState.Queue.TryDequeue(out var immediateEnvelope))
        {
            _inflight[immediateEnvelope.Id] = (queueKey, immediateEnvelope);
            return immediateEnvelope;
        }

        if (waitMs <= 0)
        {
            return null;
        }

        var acquired = await queueState.Signal.WaitAsync(waitMs, token).ConfigureAwait(false);
        if (!acquired)
        {
            return null;
        }

        if (!queueState.Queue.TryDequeue(out var envelope))
        {
            return null;
        }

        _inflight[envelope.Id] = (queueKey, envelope);
        return envelope;
    }

    public Task AcknowledgeAsync(string envelopeId, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        _inflight.TryRemove(envelopeId, out _);
        return Task.CompletedTask;
    }

    public async Task HandleProcessingErrorAsync(
        Envelope<T> envelope,
        int retryLimit,
        int retryDelayMs,
        ILogger logger,
        Exception exception,
        CancellationToken token = default)
    {
        var queueKey = GetQueueKey(queueName: null);
        if (_inflight.TryRemove(envelope.Id, out var inflight))
        {
            queueKey = inflight.QueueKey;
        }

        var queueState = GetQueueState(queueKey);

        envelope.DeliveryCount++;
        envelope.LastAttemptAt = DateTimeOffset.UtcNow;
        envelope.LastError = exception.Message;

        if (envelope.DeliveryCount > retryLimit)
        {
            queueState.DeadLetterQueue.Enqueue(envelope);
            logger.LogError(
                exception,
                "Message moved to in-memory dead-letter queue. envelopeId={EnvelopeId} messageType={MessageType}",
                envelope.Id,
                typeof(T).Name);
            return;
        }

        logger.LogWarning(
            exception,
            "Retrying in-memory message. envelopeId={EnvelopeId} attempt={Attempt}",
            envelope.Id,
            envelope.DeliveryCount);

        if (retryDelayMs > 0)
        {
            await Task.Delay(retryDelayMs, token).ConfigureAwait(false);
        }

        queueState.Queue.Enqueue(envelope);
        queueState.Signal.Release();
    }

    public int DeadLetterCount => _queues.Values.Sum(state => state.DeadLetterQueue.Count);

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

    private QueueState GetQueueState(string queueKey)
    {
        return _queues.GetOrAdd(queueKey, _ => new QueueState());
    }

    private static string GetQueueKey(string? queueName)
    {
        var messageName = typeof(T).Name.ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(queueName))
        {
            return $"queue:{messageName}";
        }

        return $"queue:{queueName.Trim().ToLowerInvariant()}:{messageName}";
    }

    private sealed class QueueState
    {
        public ConcurrentQueue<Envelope<T>> Queue { get; } = new();

        public ConcurrentQueue<Envelope<T>> DeadLetterQueue { get; } = new();

        public SemaphoreSlim Signal { get; } = new(0);
    }
}