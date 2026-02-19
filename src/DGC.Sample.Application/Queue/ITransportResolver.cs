namespace DGC.Sample.Application.Queue;

public interface ITransportResolver<T>
{
    IMessageQueueTransport<T> Resolve(QueueTransport transport);

    bool TryResolve(QueueTransport transport, out IMessageQueueTransport<T>? transportImpl);
}