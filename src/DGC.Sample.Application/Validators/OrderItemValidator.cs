using FluentValidation;
using DGC.Sample.Application.Dtos;
using DGC.Sample.Application.Interfaces;

namespace DGC.Sample.Application.Validators;

public sealed class OrderItemValidator : AbstractValidator<OrderItemRequest>
{
    private readonly IProductRepository _productRepository;

    public OrderItemValidator(IProductRepository productRepository)
    {
        _productRepository = productRepository;

        RuleFor(x => x.ProductId)
            .NotEmpty();

        RuleFor(x => x.ProductName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .LessThanOrEqualTo(100);

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0);

        // ASYNC CHECK per item: Check stock availability
        RuleFor(x => x)
            .MustAsync(async (item, ct) => 
            {
                var availableStock = await _productRepository.GetAvailableStockAsync(item.ProductId, ct);
                return item.Quantity <= availableStock;
            })
            .WithMessage(item => $"Insufficient stock for product '{item.ProductName}'. Only {item.Quantity} requested but stock is limited.")
            .WithName("Quantity");
    }
}
