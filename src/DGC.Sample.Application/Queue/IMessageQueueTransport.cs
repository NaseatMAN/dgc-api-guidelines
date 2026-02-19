using Microsoft.Extensions.Logging;

namespace DGC.Sample.Application.Queue;

public interface IMessageQueueTransport<T>
{
    QueueTransport TransportType { get; }

    Task EnqueueAsync(T item, CancellationToken token = default);

    Task<Envelope<T>?> DequeueAsync(int waitMs, CancellationToken token = default);

    Task AcknowledgeAsync(string envelopeId, CancellationToken token = default);

    Task HandleProcessingErrorAsync(
        Envelope<T> envelope,
        int retryLimit,
        int retryDelayMs,
        ILogger logger,
        Exception exception,
        CancellationToken token = default);
}