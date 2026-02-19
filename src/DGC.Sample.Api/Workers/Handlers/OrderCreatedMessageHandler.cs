using DGC.Sample.Application.Interfaces;
using DGC.Sample.Application.Queue;
using DGC.Sample.Application.Queue.Messages;

namespace DGC.Sample.Api.Workers.Handlers;

public sealed class OrderCreatedMessageHandler(IOrderService orderService, ILogger<OrderCreatedMessageHandler> logger) : IMessageHandler<OrderCreatedMessage>
{
    private readonly IOrderService _orderService = orderService;
    private readonly ILogger<OrderCreatedMessageHandler> _logger = logger;

    public async Task HandleAsync(OrderCreatedMessage message, CancellationToken token)
    {
        var order = await _orderService.GetByIdAsync(message.OrderId, token).ConfigureAwait(false);
        if (order is null)
        {
            _logger.LogWarning(
                "OrderCreatedMessage received for missing order. orderId={OrderId}",
                message.OrderId);
            return;
        }

        _logger.LogInformation(
            "Processed OrderCreatedMessage. orderId={OrderId} customer={CustomerName} totalAmount={TotalAmount}",
            message.OrderId,
            message.CustomerName,
            message.TotalAmount);
    }
}