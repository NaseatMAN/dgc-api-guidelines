using System.ComponentModel.DataAnnotations;
using DGC.Sample.Domain.Enums;

namespace DGC.Sample.Application.Dtos;

public sealed class OrderCreateRequest
{
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string CustomerName { get; init; } = string.Empty;

    [Required]
    public DateTime OrderDateUtc { get; init; }

    [Required]
    public OrderStatus Status { get; init; }

    [Range(0.01, 1_000_000)]
    public decimal TotalAmount { get; init; }
}
