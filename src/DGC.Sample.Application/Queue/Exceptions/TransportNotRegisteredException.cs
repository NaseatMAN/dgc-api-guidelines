namespace DGC.Sample.Application.Queue.Exceptions;

public sealed class TransportNotRegisteredException : InvalidOperationException
{
    public TransportNotRegisteredException(QueueTransport transport, Type messageType)
        : base($"Queue transport '{transport}' is not registered for message type '{messageType.Name}'.")
    {
    }
}