using FluentValidation;
using DGC.Sample.Application.Dtos;

namespace DGC.Sample.Application.Validators;

public sealed class OrderUpdateRequestValidator : AbstractValidator<OrderUpdateRequest>
{
    public OrderUpdateRequestValidator()
    {
        RuleFor(x => x.CustomerName)
            .NotEmpty()
            .Length(2, 200);

        RuleFor(x => x.OrderDateUtc)
            .NotEmpty();

        RuleFor(x => x.Status)
            .IsInEnum();

        RuleFor(x => x.TotalAmount)
            .GreaterThan(0)
            .LessThanOrEqualTo(1_000_000);
    }
}
