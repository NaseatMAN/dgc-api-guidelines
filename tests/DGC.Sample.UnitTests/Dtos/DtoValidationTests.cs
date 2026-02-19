using System.ComponentModel.DataAnnotations;
using DGC.Sample.Application.Dtos;
using DGC.Sample.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace DGC.Sample.UnitTests.Dtos;

public sealed class DtoValidationTests
{
    [Fact]
    public void OrderCreateRequest_WhenTotalAmountDoesNotMatchItemsSum_ShouldHaveError()
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

        // Act
        var (isValid, results) = Validate(request);

        // Assert
        isValid.Should().BeFalse();
        results.Should().Contain(e => e.MemberNames.Contains("TotalAmount") && e.ErrorMessage!.Contains("sum of item totals"));
    }

    [Fact]
    public void OrderCreateRequest_WhenShippingDateIsTooSoon_ShouldHaveError()
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

        // Act
        var (isValid, results) = Validate(request);

        // Assert
        isValid.Should().BeFalse();
        results.Should().Contain(e => e.MemberNames.Contains("ShippingDateUtc"));
    }

    [Fact]
    public void UserRegistrationRequest_WhenAdminEmailDoesNotEndWithDgc_ShouldHaveError()
    {
        // Arrange
        var request = new UserRegistrationRequest(
            "John Doe",
            "123456789",
            "012345678",
            "john@gmail.com", // Error: Should be @dgc.com for Admin
            "Admin",
            "Password123",
            "Password123"
        );

        // Act
        var (isValid, results) = Validate(request);

        // Assert
        isValid.Should().BeFalse();
        results.Should().Contain(e => e.MemberNames.Contains("Email") && e.ErrorMessage!.Contains("@dgc.com"));
    }

    [Fact]
    public void UserRegistrationRequest_WhenPasswordsDoNotMatch_ShouldHaveError()
    {
        // Arrange
        var request = new UserRegistrationRequest(
            "John Doe",
            "123456789",
            "012345678",
            "john@dgc.com",
            "Admin",
            "Password123",
            "WrongPassword" // Error: Does not match
        );

        // Act
        var (isValid, results) = Validate(request);

        // Assert
        isValid.Should().BeFalse();
        results.Should().Contain(e => e.MemberNames.Contains("ConfirmPassword"));
    }

    private static (bool IsValid, List<ValidationResult> Results) Validate(object obj)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(obj, null, null);
        var isValid = Validator.TryValidateObject(obj, context, results, true);

        // Recursively validate collections (Data Annotations don't do this automatically)
        if (obj is OrderCreateRequest orderRequest)
        {
            foreach (var item in orderRequest.Items)
            {
                var itemResults = new List<ValidationResult>();
                var itemContext = new ValidationContext(item, null, null);
                if (!Validator.TryValidateObject(item, itemContext, itemResults, true))
                {
                    isValid = false;
                    results.AddRange(itemResults);
                }
            }
        }

        return (isValid, results);
    }
}
