namespace DGC.Sample.Application.Queue;

public interface IQueueService
{
    Task EnqueueAsync<T>(
        T item,
        QueueTransport? transport = null,
        CancellationToken cancellationToken = default);

    Task<T?> DequeueAsync<T>(
        QueueTransport? transport = null,
        CancellationToken cancellationToken = default)
        where T : class;
}