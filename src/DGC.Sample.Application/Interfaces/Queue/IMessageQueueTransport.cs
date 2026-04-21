using DGC.Sample.Application.Common.Queue;
using DGC.Sample.Application.Dtos.Queue;
using Microsoft.Extensions.Logging;

namespace DGC.Sample.Application.Interfaces.Queue;

public interface IMessageQueueTransport<T>
{
    QueueTransport TransportType { get; }

    Task EnqueueAsync(T item, CancellationToken token);

    Task EnqueueAsync(T item, string? queueName, CancellationToken token);

    Task<Envelope<T>?> DequeueAsync(int waitMs, CancellationToken token);

    Task<Envelope<T>?> DequeueAsync(int waitMs, string? queueName, CancellationToken token);

    Task AcknowledgeAsync(string envelopeId, CancellationToken token);

    Task HandleProcessingErrorAsync(
        Envelope<T> envelope,
        int retryLimit,
        int retryDelayMs,
        ILogger logger,
        Exception exception,
        CancellationToken token);
}