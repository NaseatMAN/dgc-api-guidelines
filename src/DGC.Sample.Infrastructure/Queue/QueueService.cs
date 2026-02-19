using DGC.Sample.Application.Queue;
using Microsoft.Extensions.DependencyInjection;

namespace DGC.Sample.Infrastructure.Queue;

public sealed class QueueService(IServiceProvider provider) : IQueueService
{
    private readonly IServiceProvider _provider = provider;

    public async Task EnqueueAsync<T>(
        T item,
        QueueTransport transport = QueueTransport.InMemory,
        CancellationToken cancellationToken = default)
    {
        var resolver = _provider.GetRequiredService<ITransportResolver<T>>();
        var queueTransport = resolver.Resolve(transport);
        await queueTransport.EnqueueAsync(item, cancellationToken).ConfigureAwait(false);
    }

    public async Task<T?> DequeueAsync<T>(
        QueueTransport transport = QueueTransport.InMemory,
        CancellationToken cancellationToken = default)
        where T : class
    {
        var resolver = _provider.GetRequiredService<ITransportResolver<T>>();
        var queueTransport = resolver.Resolve(transport);
        var envelope = await queueTransport.DequeueAsync(0, cancellationToken).ConfigureAwait(false);
        return envelope?.Payload;
    }
}