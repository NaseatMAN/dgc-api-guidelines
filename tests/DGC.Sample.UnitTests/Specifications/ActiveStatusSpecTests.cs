using DGC.Sample.Domain.Entities;
using DGC.Sample.Domain.Specifications.Orders;
using DGC.Sample.Domain.Specifications.Users;
using FluentAssertions;
using Xunit;

namespace DGC.Sample.UnitTests.Specifications;

public sealed class ActiveStatusSpecTests
{
    private readonly List<User> _users =
    [
        new User { Id = Guid.NewGuid(), FullName = "Active User", IsActive = true },
        new User { Id = Guid.NewGuid(), FullName = "Inactive User", IsActive = false }
    ];

    private readonly List<Order> _orders =
    [
        new Order { Id = Guid.NewGuid(), CustomerName = "Active Order", IsActive = true },
        new Order { Id = Guid.NewGuid(), CustomerName = "Inactive Order", IsActive = false }
    ];

    [Fact]
    public void UserActiveStatusSpec_WhenIsActiveTrue_ShouldFilterActiveUsers()
    {
        // Arrange
        var spec = new UserActiveStatusSpec(true);
        var predicate = spec.Criteria!.Compile();

        // Act
        var result = _users.Where(predicate).ToList();

        // Assert
        result.Should().HaveCount(1);
        result.All(u => u.IsActive).Should().BeTrue();
        result[0].FullName.Should().Be("Active User");
    }

    [Fact]
    public void UserActiveStatusSpec_WhenIsActiveFalse_ShouldFilterInactiveUsers()
    {
        // Arrange
        var spec = new UserActiveStatusSpec(false);
        var predicate = spec.Criteria!.Compile();

        // Act
        var result = _users.Where(predicate).ToList();

        // Assert
        result.Should().HaveCount(1);
        result.All(u => !u.IsActive).Should().BeTrue();
        result[0].FullName.Should().Be("Inactive User");
    }

    [Fact]
    public void OrderActiveStatusSpec_WhenIsActiveTrue_ShouldFilterActiveOrders()
    {
        // Arrange
        var spec = new OrderActiveStatusSpec(true);
        var predicate = spec.Criteria!.Compile();

        // Act
        var result = _orders.Where(predicate).ToList();

        // Assert
        result.Should().HaveCount(1);
        result.All(o => o.IsActive).Should().BeTrue();
        result[0].CustomerName.Should().Be("Active Order");
    }

    [Fact]
    public void OrderActiveStatusSpec_WhenIsActiveFalse_ShouldFilterInactiveOrders()
    {
        // Arrange
        var spec = new OrderActiveStatusSpec(false);
        var predicate = spec.Criteria!.Compile();

        // Act
        var result = _orders.Where(predicate).ToList();

        // Assert
        result.Should().HaveCount(1);
        result.All(o => !o.IsActive).Should().BeTrue();
        result[0].CustomerName.Should().Be("Inactive Order");
    }
}
