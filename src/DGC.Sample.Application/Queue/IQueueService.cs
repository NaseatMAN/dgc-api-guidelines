namespace DGC.Sample.Application.Queue;

public interface IQueueService
{
    Task EnqueueAsync<T>(
        T item,
        QueueTransport transport = QueueTransport.InMemory,
        CancellationToken cancellationToken = default);

    Task<T?> DequeueAsync<T>(
        QueueTransport transport = QueueTransport.InMemory,
        CancellationToken cancellationToken = default)
        where T : class;
}