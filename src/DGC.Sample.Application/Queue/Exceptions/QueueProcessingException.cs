namespace DGC.Sample.Application.Queue.Exceptions;

public sealed class QueueProcessingException : Exception
{
    public QueueProcessingException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}