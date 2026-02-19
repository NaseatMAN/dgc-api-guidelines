using DGC.Sample.Domain.Enums;

namespace DGC.Sample.Application.Dtos;

public sealed class OrderUpdateRequest
{
    public string CustomerName { get; init; } = string.Empty;

    public DateTime OrderDateUtc { get; init; }

    public OrderStatus Status { get; init; }

    public decimal TotalAmount { get; init; }
}
