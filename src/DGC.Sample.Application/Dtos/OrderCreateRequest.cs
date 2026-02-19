using DGC.Sample.Domain.Enums;

namespace DGC.Sample.Application.Dtos;

public sealed class OrderCreateRequest
{
    public string CustomerName { get; init; } = string.Empty;

    public DateTime OrderDateUtc { get; init; }

    public DateTime ShippingDateUtc { get; init; }

    public OrderStatus Status { get; init; }

    public decimal TotalAmount { get; init; }

    public string? DiscountCode { get; init; }

    public List<OrderItemRequest> Items { get; init; } = [];
}

public sealed record OrderItemRequest(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice);
