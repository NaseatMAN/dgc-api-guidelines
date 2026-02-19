using DGC.Sample.Application.Queue;
using DGC.Sample.Application.Queue.Messages;

namespace DGC.Sample.Api.Workers;

public sealed class BackgroundOrderCreatedWorker(IServiceScopeFactory scopeFactory, ILogger<MessageProcessingServiceBase<OrderCreatedMessage>> logger)
    : MessageProcessingServiceBase<OrderCreatedMessage>(scopeFactory, logger)
{
    protected override string WorkerName => "BackgroundOrderCreated";

    protected override async Task ProcessMessageAsync(
        IServiceProvider serviceProvider,
        OrderCreatedMessage message,
        CancellationToken token)
    {
        var handler = serviceProvider.GetRequiredService<IMessageHandler<OrderCreatedMessage>>();
        await handler.HandleAsync(message, token).ConfigureAwait(false);
    }
}