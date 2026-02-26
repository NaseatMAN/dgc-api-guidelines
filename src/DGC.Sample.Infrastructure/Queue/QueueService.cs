using DGC.Sample.Application.Queue;
using Microsoft.Extensions.DependencyInjection;

namespace DGC.Sample.Infrastructure.Queue;

public sealed class QueueService(IServiceProvider provider, QueueServiceOptions options) : IQueueService
{
    private readonly IServiceProvider _provider = provider;
    private readonly QueueServiceOptions _options = options;

    public async Task EnqueueAsync<T>(
        T item,
        QueueTransport? transport = null,
        CancellationToken cancellationToken = default)
    {
        await EnqueueAsync(item, transport, queueName: null, cancellationToken).ConfigureAwait(false);
    }

    public async Task EnqueueAsync<T>(
        T item,
        QueueTransport? transport,
        string? queueName,
        CancellationToken cancellationToken = default)
    {
        var selectedTransport = transport ?? _options.DefaultTransport;
        var resolver = _provider.GetRequiredService<ITransportResolver<T>>();
        var queueTransport = resolver.Resolve(selectedTransport);
        await queueTransport.EnqueueAsync(item, queueName, cancellationToken).ConfigureAwait(false);
    }

    public async Task<T?> DequeueAsync<T>(
        QueueTransport? transport = null,
        CancellationToken cancellationToken = default)
        where T : class
    {
        return await DequeueAsync<T>(transport, queueName: null, cancellationToken).ConfigureAwait(false);
    }

    public async Task<T?> DequeueAsync<T>(
        QueueTransport? transport,
        string? queueName,
        CancellationToken cancellationToken = default)
        where T : class
    {
        var selectedTransport = transport ?? _options.DefaultTransport;
        var resolver = _provider.GetRequiredService<ITransportResolver<T>>();
        var queueTransport = resolver.Resolve(selectedTransport);
        var envelope = await queueTransport.DequeueAsync(0, queueName, cancellationToken).ConfigureAwait(false);
        return envelope?.Payload;
    }
}