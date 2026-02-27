namespace DGC.Sample.Application.Interfaces.Queue;

public interface IMessageHandler<T>
{
    Task HandleAsync(T message, CancellationToken token);
}