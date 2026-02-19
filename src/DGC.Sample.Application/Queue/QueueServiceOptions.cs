namespace DGC.Sample.Application.Queue;

public sealed class QueueServiceOptions
{
    public QueueTransport DefaultTransport { get; init; } = QueueTransport.InMemory;
}