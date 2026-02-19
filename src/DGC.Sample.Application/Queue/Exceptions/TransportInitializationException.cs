namespace DGC.Sample.Application.Queue.Exceptions;

public sealed class TransportInitializationException : InvalidOperationException
{
    public TransportInitializationException(string message)
        : base(message)
    {
    }
}