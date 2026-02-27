using DGC.Sample.Application.Common.Queue;
using DGC.Sample.Application.Dtos.Queue;
using DGC.Sample.Application.Interfaces.Queue;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DGC.Sample.Application.Services.Queue.Workers;

public sealed class BackgroundOrderCreatedRedisWorker(
    IServiceScopeFactory scopeFactory,
    ITransportResolver<OrderCreatedMessage> transportResolver,
    ILogger<MessageProcessingServiceBase<OrderCreatedMessage>> logger)
    : MessageProcessingServiceBase<OrderCreatedMessage>(
        scopeFactory,
        transportResolver,
        logger)
{
    protected override string WorkerName => "BackgroundOrderCreatedRedis";

    protected override QueueTransport Transport => QueueTransport.Redis;

    protected override async Task ProcessMessageAsync(
        IServiceProvider serviceProvider,
        OrderCreatedMessage message,
        CancellationToken token)
    {
        var handler = serviceProvider.GetRequiredService<IMessageHandler<OrderCreatedMessage>>();
        await handler.HandleAsync(message, token).ConfigureAwait(false);
    }
}