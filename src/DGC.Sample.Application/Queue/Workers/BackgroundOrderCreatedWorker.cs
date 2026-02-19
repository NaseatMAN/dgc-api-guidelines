using DGC.Sample.Application.Queue;
using DGC.Sample.Application.Queue.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DGC.Sample.Application.Queue.Workers;

public sealed class BackgroundOrderCreatedWorker(
    IServiceScopeFactory scopeFactory,
    ITransportResolver<OrderCreatedMessage> transportResolver,
    ILogger<MessageProcessingServiceBase<OrderCreatedMessage>> logger)
    : MessageProcessingServiceBase<OrderCreatedMessage>(
        scopeFactory,
        transportResolver,
        logger)
{
    protected override string WorkerName => "BackgroundOrderCreated";

    protected override QueueTransport Transport => QueueTransport.InMemory;

    protected override async Task ProcessMessageAsync(
        IServiceProvider serviceProvider,
        OrderCreatedMessage message,
        CancellationToken token)
    {
        var handler = serviceProvider.GetRequiredService<IMessageHandler<OrderCreatedMessage>>();
        await handler.HandleAsync(message, token).ConfigureAwait(false);
    }
}