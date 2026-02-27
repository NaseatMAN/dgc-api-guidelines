using DGC.Sample.Application.Validators;
using DGC.Sample.Domain.Enums;

namespace DGC.Sample.Application.Dtos;

public sealed class OrderUpdateRequest
{
    [NotWhiteSpace]
    [StringLengthRange(2, 200)]
    public string CustomerName { get; init; } = string.Empty;

    [NonDefaultDateTime]
    public DateTime OrderDateUtc { get; init; }

    [EnumDefined]
    public OrderStatus Status { get; init; }

    [DecimalRangeExclusiveMin(0, 1_000_000)]
    public decimal TotalAmount { get; init; }
}
