using DGC.Sample.Application.Dtos;
using DGC.Sample.Domain.Entities;
using DGC.Sample.Domain.Enums;

namespace DGC.Sample.Application.Mappings;

public static class OrderMapper
{
    public static OrderResponse ToResponse(Order order)
    {
        return new OrderResponse
        {
            Id = order.Id,
            CustomerName = order.CustomerName,
            OrderDateUtc = order.OrderDateUtc,
            Status = (OrderStatus)order.Status,
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
            Status = (int)request.Status,
            IsDeleted = false,
            TotalAmount = request.TotalAmount
        };
    }
}
