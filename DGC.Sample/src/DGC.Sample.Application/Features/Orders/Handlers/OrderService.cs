using DGC.Sample.Application.Abstractions.Interfaces;
using DGC.Sample.Application.Features.Orders.Dtos;
using DGC.Sample.Application.Mappers;
using DGC.Sample.Domain.Entities;

namespace DGC.Sample.Application.Features.Orders.Handlers;

public sealed class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;

    public OrderService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<IReadOnlyList<OrderResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetAllAsync(cancellationToken);
        return orders.Select(OrderMapper.ToResponse).ToArray();
    }

    public async Task<OrderResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken);
        return order is null ? null : OrderMapper.ToResponse(order);
    }

    public async Task<OrderResponse> CreateAsync(OrderCreateRequest request, CancellationToken cancellationToken)
    {
        var order = OrderMapper.ToEntity(Guid.NewGuid(), request);
        await _orderRepository.AddAsync(order, cancellationToken);
        return OrderMapper.ToResponse(order);
    }

    public async Task<OrderResponse?> UpdateAsync(Guid id, OrderUpdateRequest request, CancellationToken cancellationToken)
    {
        var existing = await _orderRepository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        existing.CustomerName = request.CustomerName;
        existing.OrderDateUtc = request.OrderDateUtc;
        existing.Status = request.Status;
        existing.TotalAmount = request.TotalAmount;

        await _orderRepository.UpdateAsync(existing, cancellationToken);
        return OrderMapper.ToResponse(existing);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var existing = await _orderRepository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return false;
        }

        await _orderRepository.DeleteAsync(id, cancellationToken);
        return true;
    }
}
