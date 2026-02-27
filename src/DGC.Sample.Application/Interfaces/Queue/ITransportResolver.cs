using DGC.Sample.Application.Common.Queue;

namespace DGC.Sample.Application.Interfaces.Queue;

public interface ITransportResolver<T>
{
    IMessageQueueTransport<T> Resolve(QueueTransport transport);

    bool TryResolve(QueueTransport transport, out IMessageQueueTransport<T>? transportImpl);
}