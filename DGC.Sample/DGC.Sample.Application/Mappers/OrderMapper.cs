using DGC.Sample.Application.Features.Orders.Dtos;
using DGC.Sample.Domain.Entities;

namespace DGC.Sample.Application.Mappers;

public static class OrderMapper
{
    public static OrderResponse ToResponse(Order order)
    {
        return new OrderResponse
        {
            Id = order.Id,
            CustomerName = order.CustomerName,
            OrderDateUtc = order.OrderDateUtc,
            Status = order.Status,
            TotalAmount = order.TotalAmount
        };
    }

    public static Order ToEntity(Guid id, OrderCreateRequest request)
    {
        return new Order
        {
            Id = id,
            CustomerName = request.CustomerName,
            OrderDateUtc = request.OrderDateUtc,
            Status = request.Status,
            TotalAmount = request.TotalAmount
        };
    }
}
