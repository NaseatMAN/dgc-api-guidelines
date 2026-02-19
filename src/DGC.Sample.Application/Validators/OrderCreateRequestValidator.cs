using FluentValidation;
using DGC.Sample.Application.Dtos;
using DGC.Sample.Application.Interfaces;

namespace DGC.Sample.Application.Validators;

public sealed class OrderCreateRequestValidator : AbstractValidator<OrderCreateRequest>
{
    public OrderCreateRequestValidator(IProductRepository productRepository)
    {
        RuleFor(x => x.CustomerName)
            .NotEmpty()
            .Length(2, 200);

        RuleFor(x => x.OrderDateUtc)
            .NotEmpty();

        // CROSS-FIELD DATE LOGIC: Shipping must be at least 24h after Order Date
        RuleFor(x => x.ShippingDateUtc)
            .NotEmpty()
            .Must((request, shippingDate) => shippingDate >= request.OrderDateUtc.AddHours(24))
            .WithMessage("Shipping date must be at least 24 hours after the order date.");

        RuleFor(x => x.Status)
            .IsInEnum();

        // COLLECTION VALIDATION
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Order must contain at least one item.")
            .Must(items => items.Count <= 50).WithMessage("Order cannot contain more than 50 items.");

        RuleForEach(x => x.Items)
            .SetValidator(new OrderItemValidator(productRepository));

        // CROSS-COLLECTION MATH: Sum check
        RuleFor(x => x.TotalAmount)
            .Equal(x => x.Items.Sum(item => item.Quantity * item.UnitPrice))
            .WithMessage("Total amount must match the sum of item totals (Quantity * UnitPrice).");

        // COMPLEX CONDITIONAL LOGIC: Discount Code
        RuleFor(x => x.DiscountCode)
            .NotEmpty().When(x => x.TotalAmount > 1000)
            .WithMessage("A discount code is mandatory for orders over $1,000.");

        RuleFor(x => x.DiscountCode)
            .Empty().When(x => x.TotalAmount < 100)
            .WithMessage("Discount codes cannot be applied to orders under $100.");
    }
}
