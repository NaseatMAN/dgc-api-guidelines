using DGC.Sample.Domain.Enums;

namespace DGC.Sample.Application.Features.Orders.Commands;

public sealed record UpdateOrderCommand(
    Guid Id,
    string CustomerName,
    DateTime OrderDateUtc,
    OrderStatus Status,
    decimal TotalAmount);
