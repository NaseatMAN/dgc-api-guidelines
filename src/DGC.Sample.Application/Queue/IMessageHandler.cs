namespace DGC.Sample.Application.Queue;

public interface IMessageHandler<T>
{
    Task HandleAsync(T message, CancellationToken token);
}