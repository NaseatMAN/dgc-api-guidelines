using DGC.Sample.Application.Dtos;
using DGC.Sample.Application.Interfaces;
using DGC.Sample.Application.Interfaces.Persistence;
using DGC.Sample.Application.Interfaces.Repositories;
using DGC.Sample.Application.Mappings;
using DGC.Sample.Domain.Entities;
using DGC.Sample.Domain.Specifications.Orders;

namespace DGC.Sample.Application.Services;

public sealed class OrderService(IUnitOfWork unitOfWork) : IOrderRepository
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<IReadOnlyList<OrderResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var entityRepository = _unitOfWork.GetEntityRepository<Order>();
        var orders = entityRepository.QueryAsNoTracking()
            .OrderBy(order => order.OrderDateUtc)
            .ToList();
        return [.. orders.Select(OrderMapper.ToResponse)];
    }

    public async Task<IReadOnlyList<OrderResponse>> GetAllIncludingDeletedAsync(CancellationToken cancellationToken)
    {
        var spec = new OrderIncludingDeletedSpec();
        var entityRepository = _unitOfWork.GetEntityRepository<Order>();
        var orders = await entityRepository.GetListAsync(spec, cancellationToken);
        return [.. orders.Select(OrderMapper.ToResponse)];
    }

    public async Task<OrderResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var entityRepository = _unitOfWork.GetEntityRepository<Order>();
        var order = await entityRepository.FindFirstAsync(order => order.Id == id, cancellationToken);
        return order is null ? null : OrderMapper.ToResponse(order);
    }

    public async Task<OrderResponse> CreateAsync(OrderCreateRequest request, CancellationToken cancellationToken)
    {
        var entityRepository = _unitOfWork.GetEntityRepository<Order>();
        var order = OrderMapper.ToEntity(Guid.NewGuid(), request);
        entityRepository.Add(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OrderMapper.ToResponse(order);
    }

    public async Task<(OrderResponse Response, bool Created)> UpsertAsync(Guid id, OrderUpdateRequest request, CancellationToken cancellationToken)
    {
        var entityRepository = _unitOfWork.GetEntityRepository<Order>();
        var existing = await entityRepository.FindFirstAsync(order => order.Id == id, cancellationToken);
        if (existing is null)
        {
            var newOrder = OrderMapper.ToEntity(id, new OrderCreateRequest
            {
                CustomerName = request.CustomerName,
                OrderDateUtc = request.OrderDateUtc,
                Status = request.Status,
                TotalAmount = request.TotalAmount
            });
            entityRepository.Add(newOrder);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return (OrderMapper.ToResponse(newOrder), true);
        }

        existing.CustomerName = request.CustomerName;
        existing.OrderDateUtc = request.OrderDateUtc;
        existing.Status = request.Status;
        existing.TotalAmount = request.TotalAmount;

        entityRepository.Update(existing);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return (OrderMapper.ToResponse(existing), false);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entityRepository = _unitOfWork.GetEntityRepository<Order>();
        var order = await entityRepository.FindFirstAsync(order => order.Id == id, cancellationToken);
        if (order != null)
        {
            entityRepository.Delete(order);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
