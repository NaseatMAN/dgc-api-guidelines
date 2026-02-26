using DGC.Sample.Application.Dtos;
using DGC.Sample.Application.Interfaces;
using DGC.Sample.Application.Interfaces.Persistence;
using DGC.Sample.Application.Interfaces.Services;
using DGC.Sample.Application.Mappings;
using DGC.Sample.Domain.Specifications.Orders;

namespace DGC.Sample.Application.Services;

public sealed class OrderService(IOrderRepository orderRepository, IUnitOfWork unitOfWork) : IOrderService
{
    private readonly IOrderRepository _orderRepository = orderRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<IReadOnlyList<OrderResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetAllAsync(cancellationToken);
        return [.. orders.Select(OrderMapper.ToResponse)];
    }

    public async Task<IReadOnlyList<OrderResponse>> GetAllIncludingDeletedAsync(CancellationToken cancellationToken)
    {
        var spec = new OrderIncludingDeletedSpec();
        var orders = await _orderRepository.GetListAsync(spec, cancellationToken);
        return [.. orders.Select(OrderMapper.ToResponse)];
    }

    public async Task<OrderResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken);
        return order is null ? null : OrderMapper.ToResponse(order);
    }

    public async Task<OrderResponse> CreateAsync(OrderCreateRequest request, CancellationToken cancellationToken)
    {
        var order = OrderMapper.ToEntity(Guid.NewGuid(), request);
        _orderRepository.Add(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OrderMapper.ToResponse(order);
    }

    public async Task<(OrderResponse Response, bool Created)> UpsertAsync(Guid id, OrderUpdateRequest request, CancellationToken cancellationToken)
    {
        var existing = await _orderRepository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            var newOrder = OrderMapper.ToEntity(id, new OrderCreateRequest
            {
                CustomerName = request.CustomerName,
                OrderDateUtc = request.OrderDateUtc,
                Status = request.Status,
                TotalAmount = request.TotalAmount
            });
            _orderRepository.Add(newOrder);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return (OrderMapper.ToResponse(newOrder), true);
        }

        existing.CustomerName = request.CustomerName;
        existing.OrderDateUtc = request.OrderDateUtc;
        existing.Status = request.Status;
        existing.TotalAmount = request.TotalAmount;

        _orderRepository.Update(existing);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return (OrderMapper.ToResponse(existing), false);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken);
        if (order != null)
        {
            _orderRepository.Delete(order);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
