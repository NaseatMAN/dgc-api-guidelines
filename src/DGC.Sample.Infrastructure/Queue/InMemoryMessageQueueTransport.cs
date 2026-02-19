using System.Collections.Concurrent;
using DGC.Sample.Application.Queue;
using Microsoft.Extensions.Logging;

namespace DGC.Sample.Infrastructure.Queue;

public sealed class InMemoryMessageQueueTransport<T> : IMessageQueueTransport<T>
{
    private readonly ConcurrentQueue<Envelope<T>> _queue = new();
    private readonly ConcurrentDictionary<string, Envelope<T>> _inflight = new();
    private readonly ConcurrentQueue<Envelope<T>> _deadLetterQueue = new();
    private readonly SemaphoreSlim _signal = new(0);

    public QueueTransport TransportType => QueueTransport.InMemory;

    public Task EnqueueAsync(T item, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        var envelope = CreateEnvelope(item);
        _queue.Enqueue(envelope);
        _signal.Release();

        return Task.CompletedTask;
    }

    public async Task<Envelope<T>?> DequeueAsync(int waitMs, CancellationToken token = default)
    {
        if (_queue.TryDequeue(out var immediateEnvelope))
        {
            _inflight[immediateEnvelope.Id] = immediateEnvelope;
            return immediateEnvelope;
        }

        if (waitMs <= 0)
        {
            return null;
        }

        var acquired = await _signal.WaitAsync(waitMs, token).ConfigureAwait(false);
        if (!acquired)
        {
            return null;
        }

        if (!_queue.TryDequeue(out var envelope))
        {
            return null;
        }

        _inflight[envelope.Id] = envelope;
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
        _inflight.TryRemove(envelope.Id, out _);

        envelope.DeliveryCount++;
        envelope.LastAttemptAt = DateTimeOffset.UtcNow;
        envelope.LastError = exception.Message;

        if (envelope.DeliveryCount > retryLimit)
        {
            _deadLetterQueue.Enqueue(envelope);
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

        _queue.Enqueue(envelope);
        _signal.Release();
    }

    public int DeadLetterCount => _deadLetterQueue.Count;

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
}