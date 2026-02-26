namespace DGC.Sample.Application.Queue;

public interface IQueueService
{
    Task EnqueueAsync<T>(
        T item,
        QueueTransport? transport = null,
        CancellationToken cancellationToken = default);

    Task EnqueueAsync<T>(
        T item,
        QueueTransport? transport,
        string? queueName,
        CancellationToken cancellationToken = default);

    Task<T?> DequeueAsync<T>(
        QueueTransport? transport = null,
        CancellationToken cancellationToken = default)
        where T : class;

    Task<T?> DequeueAsync<T>(
        QueueTransport? transport,
        string? queueName,
        CancellationToken cancellationToken = default)
        where T : class;
}