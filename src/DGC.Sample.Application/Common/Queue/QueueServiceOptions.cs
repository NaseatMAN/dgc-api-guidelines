namespace DGC.Sample.Application.Common.Queue;

public sealed class QueueServiceOptions
{
    public QueueTransport DefaultTransport { get; init; } = QueueTransport.InMemory;
}