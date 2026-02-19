using DGC.Sample.Application.Dtos;
using DGC.Sample.Application.Interfaces;
using DGC.Sample.Application.Validators;
using DGC.Sample.Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace DGC.Sample.UnitTests.Validators;

public sealed class OrderCreateRequestValidatorTests
{
    private readonly IProductRepository _productRepository;
    private readonly OrderCreateRequestValidator _validator;

    public OrderCreateRequestValidatorTests()
    {
        _productRepository = Substitute.For<IProductRepository>();
        _validator = new OrderCreateRequestValidator(_productRepository);
    }

    [Fact]
    public async Task ValidateAsync_WhenTotalAmountDoesNotMatchItemsSum_ShouldHaveError()
    {
        // Arrange
        var request = new OrderCreateRequest
        {
            CustomerName = "John Doe",
            OrderDateUtc = DateTime.UtcNow,
            ShippingDateUtc = DateTime.UtcNow.AddDays(2),
            Status = OrderStatus.Draft,
            TotalAmount = 500, // Error: 10 * 40 = 400, not 500
            Items = [new OrderItemRequest(Guid.NewGuid(), "Laptop", 10, 40)]
        };

        _productRepository.GetAvailableStockAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(100);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "TotalAmount" && e.ErrorMessage.Contains("sum of item totals"));
    }

    [Fact]
    public async Task ValidateAsync_WhenInsufficientStock_ShouldHaveError()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var request = new OrderCreateRequest
        {
            CustomerName = "John Doe",
            OrderDateUtc = DateTime.UtcNow,
            ShippingDateUtc = DateTime.UtcNow.AddDays(2),
            Status = OrderStatus.Draft,
            TotalAmount = 500,
            Items = [new OrderItemRequest(productId, "Laptop", 10, 50)]
        };

        _productRepository.GetAvailableStockAsync(productId, Arg.Any<CancellationToken>())
            .Returns(5); // Only 5 in stock, but 10 requested

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Insufficient stock"));
    }

    [Fact]
    public async Task ValidateAsync_WhenShippingDateIsTooSoon_ShouldHaveError()
    {
        // Arrange
        var request = new OrderCreateRequest
        {
            CustomerName = "John Doe",
            OrderDateUtc = DateTime.UtcNow,
            ShippingDateUtc = DateTime.UtcNow.AddHours(12), // Error: Less than 24h
            Status = OrderStatus.Draft,
            TotalAmount = 100,
            Items = [new OrderItemRequest(Guid.NewGuid(), "Mouse", 1, 100)]
        };

        _productRepository.GetAvailableStockAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(100);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ShippingDateUtc");
    }

    [Fact]
    public async Task ValidateAsync_WhenHighValueOrderMissingDiscountCode_ShouldHaveError()
    {
        // Arrange
        var request = new OrderCreateRequest
        {
            CustomerName = "John Doe",
            OrderDateUtc = DateTime.UtcNow,
            ShippingDateUtc = DateTime.UtcNow.AddDays(2),
            Status = OrderStatus.Draft,
            TotalAmount = 1500,
            DiscountCode = null, // Error: Mandatory for > $1000
            Items = [new OrderItemRequest(Guid.NewGuid(), "High-end PC", 1, 1500)]
        };

        _productRepository.GetAvailableStockAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(100);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DiscountCode");
    }

    [Fact]
    public async Task ValidateAsync_WhenAllRulesMet_ShouldBeValid()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var request = new OrderCreateRequest
        {
            CustomerName = "John Doe",
            OrderDateUtc = DateTime.UtcNow,
            ShippingDateUtc = DateTime.UtcNow.AddDays(2),
            Status = OrderStatus.Draft,
            TotalAmount = 200,
            DiscountCode = null,
            Items = [new OrderItemRequest(productId, "Keyboard", 2, 100)]
        };

        _productRepository.GetAvailableStockAsync(productId, Arg.Any<CancellationToken>())
            .Returns(50);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
