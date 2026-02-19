using DGC.Sample.Application.Dtos;
using DGC.Sample.Application.Interfaces;
using DGC.Sample.Application.Mappings;
using DGC.Sample.Domain.Constants.ApiErrorConstants;
using DGC.Sample.Domain.Entities;
using DGC.Sample.Domain.Exceptions;
using DGC.Sample.Domain.Exceptions.Errors;

namespace DGC.Sample.Application.Services;

public sealed class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;

    public OrderService(IOrderRepository orderRepository, IProductRepository productRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
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
        await ValidateStockAsync(request.Items, cancellationToken);

        var order = OrderMapper.ToEntity(Guid.NewGuid(), request);
        await _orderRepository.AddAsync(order, cancellationToken);
        return OrderMapper.ToResponse(order);
    }

    public async Task<(OrderResponse Response, bool Created)> UpsertAsync(Guid id, OrderUpdateRequest request, CancellationToken cancellationToken)
    {
        var existing = await _orderRepository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            var createRequest = new OrderCreateRequest
            {
                CustomerName = request.CustomerName,
                OrderDateUtc = request.OrderDateUtc,
                Status = request.Status,
                TotalAmount = request.TotalAmount,
                // In a real scenario, we might need items here too, but for this sample
                // let's assume we validate what we have if applicable.
            };

            var newOrder = OrderMapper.ToEntity(id, createRequest);
            await _orderRepository.AddAsync(newOrder, cancellationToken);
            return (OrderMapper.ToResponse(newOrder), true);
        }

        existing.CustomerName = request.CustomerName;
        existing.OrderDateUtc = request.OrderDateUtc;
        existing.Status = request.Status;
        existing.TotalAmount = request.TotalAmount;

        await _orderRepository.UpdateAsync(existing, cancellationToken);
        return (OrderMapper.ToResponse(existing), false);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await _orderRepository.DeleteAsync(id, cancellationToken);
    }

    private async Task ValidateStockAsync(List<OrderItemRequest> items, CancellationToken cancellationToken)
    {
        var errors = new List<AzureErrorDetail>();

        foreach (var item in items)
        {
            var availableStock = await _productRepository.GetAvailableStockAsync(item.ProductId, cancellationToken);
            if (item.Quantity > availableStock)
            {
                errors.Add(new AzureErrorDetail(
                    Code: $"{BadRequestErrorCode.InvalidModelError}.Quantity",
                    Message: $"Insufficient stock for product '{item.ProductName}'. Only {availableStock} available but {item.Quantity} requested.",
                    Target: "Items"));
            }
        }

        if (errors.Count > 0)
        {
            throw new BadRequestException(
                code: BadRequestErrorCode.InvalidModelError,
                message: "One or more validation errors occurred.",
                azureErrorDetails: errors);
        }
    }
}
