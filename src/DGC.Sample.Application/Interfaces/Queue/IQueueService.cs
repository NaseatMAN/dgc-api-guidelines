using DGC.Sample.Application.Common.Queue;

namespace DGC.Sample.Application.Interfaces.Queue;

public interface IQueueService
{
    Task EnqueueAsync<T>(
        T item,
        QueueTransport? transport,
        CancellationToken cancellationToken);

    Task EnqueueAsync<T>(
        T item,
        QueueTransport? transport,
        string? queueName,
        CancellationToken cancellationToken);

    Task<T?> DequeueAsync<T>(
        QueueTransport? transport,
        CancellationToken cancellationToken)
        where T : class;

    Task<T?> DequeueAsync<T>(
        QueueTransport? transport,
        string? queueName,
        CancellationToken cancellationToken)
        where T : class;
}