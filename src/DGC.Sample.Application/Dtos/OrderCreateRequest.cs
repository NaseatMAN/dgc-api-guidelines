using System.ComponentModel.DataAnnotations;
using DGC.Sample.Domain.Enums;

namespace DGC.Sample.Application.Dtos;

public sealed class OrderCreateRequest : IValidatableObject
{
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string CustomerName { get; init; } = string.Empty;

    [Required]
    public DateTime OrderDateUtc { get; init; }

    [Required]
    public DateTime ShippingDateUtc { get; init; }

    [Required]
    [EnumDataType(typeof(OrderStatus))]
    public OrderStatus Status { get; init; }

    [Required]
    [Range(0, 1_000_000)]
    public decimal TotalAmount { get; init; }

    public string? DiscountCode { get; init; }

    [Required]
    [MinLength(1, ErrorMessage = "Order must contain at least one item.")]
    [MaxLength(50, ErrorMessage = "Order cannot contain more than 50 items.")]
    public List<OrderItemRequest> Items { get; init; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // CROSS-FIELD DATE LOGIC: Shipping must be at least 24h after Order Date
        if (ShippingDateUtc < OrderDateUtc.AddHours(24))
        {
            yield return new ValidationResult(
                "Shipping date must be at least 24 hours after the order date.",
                [nameof(ShippingDateUtc)]);
        }

        // CROSS-COLLECTION MATH: Sum check
        var calculatedTotal = Items.Sum(item => item.Quantity * item.UnitPrice);
        if (TotalAmount != calculatedTotal)
        {
            yield return new ValidationResult(
                "Total amount must match the sum of item totals (Quantity * UnitPrice).",
                [nameof(TotalAmount)]);
        }

        // COMPLEX CONDITIONAL LOGIC: Discount Code
        if (TotalAmount > 1000 && string.IsNullOrWhiteSpace(DiscountCode))
        {
            yield return new ValidationResult(
                "A discount code is mandatory for orders over $1,000.",
                [nameof(DiscountCode)]);
        }

        if (TotalAmount < 100 && !string.IsNullOrWhiteSpace(DiscountCode))
        {
            yield return new ValidationResult(
                "Discount codes cannot be applied to orders under $100.",
                [nameof(DiscountCode)]);
        }
    }
}

public sealed record OrderItemRequest(
    [Required] Guid ProductId,
    [Required][StringLength(200)] string ProductName,
    [Required][Range(1, 100)] int Quantity,
    [Required][Range(0.01, double.MaxValue)] decimal UnitPrice);
