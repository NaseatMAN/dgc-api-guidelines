using DGC.Sample.Domain.Enums;

namespace DGC.Sample.Application.Features.Orders.Dtos;

public sealed class OrderResponse
{
    public Guid Id { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public DateTime OrderDateUtc { get; init; }
    public OrderStatus Status { get; init; }
    public decimal TotalAmount { get; init; }
}
