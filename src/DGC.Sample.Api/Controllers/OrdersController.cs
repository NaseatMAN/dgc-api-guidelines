using Asp.Versioning;
using DGC.Sample.Api.Filters;
using DGC.Sample.Application.Dtos;
using DGC.Sample.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DGC.Sample.Api.Controllers;

[ApiController]
[ApiVersion("2026-02-05")]
[Route("orders")]
public sealed class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
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

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _orderService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
