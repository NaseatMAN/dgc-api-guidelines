using Asp.Versioning;
using DGC.Sample.Api.Filters;
using DGC.Sample.Application.Dtos;
using DGC.Sample.Application.Interfaces;
using DGC.Sample.Application.Queue;
using DGC.Sample.Application.Queue.Messages;
using Microsoft.AspNetCore.Mvc;

namespace DGC.Sample.Api.Controllers;

[ApiController]
[ApiVersion("2026-02-05")]
[Route("orders")]
public sealed class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IQueueService _queueService;

    public OrdersController(IOrderService orderService, IQueueService queueService)
    {
        _orderService = orderService;
        _queueService = queueService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<OrderResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var orders = await _orderService.GetAllAsync(cancellationToken);
        return Ok(orders);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var order = await _orderService.GetByIdAsync(id, cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost]
    [ServiceFilter(typeof(IdempotencyFilter))]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] OrderCreateRequest request, CancellationToken cancellationToken)
    {
        var created = await _orderService.CreateAsync(request, cancellationToken);

        var message = new OrderCreatedMessage(
            created.Id,
            created.CustomerName,
            created.TotalAmount,
            DateTimeOffset.UtcNow);

        await _queueService.EnqueueAsync(message, cancellationToken: cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [ServiceFilter(typeof(IdempotencyFilter))]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Update(Guid id, [FromBody] OrderUpdateRequest request, CancellationToken cancellationToken)
    {
        var (response, created) = await _orderService.UpsertAsync(id, request, cancellationToken);
        return created 
            ? CreatedAtAction(nameof(GetById), new { id = response.Id }, response) 
            : Ok(response);
    }

    [HttpPost("{id:guid}/publish-azure")]
    [ServiceFilter(typeof(IdempotencyFilter))]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PublishToAzureQueue(
        Guid id,
        [FromQuery] string? queueName,
        CancellationToken cancellationToken)
    {
        var order = await _orderService.GetByIdAsync(id, cancellationToken);
        if (order is null)
        {
            return NotFound();
        }

        var message = new OrderCreatedMessage(
            order.Id,
            order.CustomerName,
            order.TotalAmount,
            new DateTimeOffset(DateTime.SpecifyKind(order.OrderDateUtc, DateTimeKind.Utc)));

        await _queueService.EnqueueAsync(message, QueueTransport.AzureQueue, queueName, cancellationToken);

        return Accepted(new
        {
            orderId = order.Id,
            transport = QueueTransport.AzureQueue.ToString(),
            queueName,
            queuedAtUtc = DateTimeOffset.UtcNow
        });
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _orderService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
