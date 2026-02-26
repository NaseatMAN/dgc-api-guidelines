using DGC.Sample.Domain.Entities;
using DGC.Sample.Domain.Enums;
using DGC.Sample.Domain.Specifications.Orders;
using FluentAssertions;
using Xunit;

namespace DGC.Sample.UnitTests.Specifications;

public sealed class OrderFilterSpecTests
{
    private readonly List<Order> _orders =
    [
        new Order { Id = Guid.NewGuid(), CustomerName = "John Doe", Status = OrderStatus.Draft, OrderDateUtc = DateTime.UtcNow.AddDays(-2) },
        new Order { Id = Guid.NewGuid(), CustomerName = "Jane Smith", Status = OrderStatus.Fulfilled, OrderDateUtc = DateTime.UtcNow.AddDays(-1) },
        new Order { Id = Guid.NewGuid(), CustomerName = "Bob Johnson", Status = OrderStatus.Cancelled, OrderDateUtc = DateTime.UtcNow },
        new Order { Id = Guid.NewGuid(), CustomerName = "Alice Doe", Status = OrderStatus.Draft, OrderDateUtc = DateTime.UtcNow.AddDays(-3) }
    ];

    [Fact]
    public void Criteria_WhenStatusProvided_ShouldFilterByStatus()
    {
        // Arrange
        var spec = new OrderFilterSpec(OrderStatus.Draft, null);
        var predicate = spec.Criteria!.Compile();

        // Act
        var result = _orders.Where(predicate).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.All(o => o.Status == OrderStatus.Draft).Should().BeTrue();
    }

    [Fact]
    public void Criteria_WhenCustomerNameSearchProvided_ShouldFilterByCustomerName()
    {
        // Arrange
        var spec = new OrderFilterSpec(null, "Doe");
        var predicate = spec.Criteria!.Compile();

        // Act
        var result = _orders.Where(predicate).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.All(o => o.CustomerName.Contains("Doe")).Should().BeTrue();
    }

    [Fact]
    public void Criteria_WhenBothStatusAndCustomerNameProvided_ShouldFilterByBoth()
    {
        // Arrange
        var spec = new OrderFilterSpec(OrderStatus.Draft, "John");
        var predicate = spec.Criteria!.Compile();

        // Act
        var result = _orders.Where(predicate).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].CustomerName.Should().Be("John Doe");
    }

    [Fact]
    public void OrderBy_ShouldBeDescendingByDate()
    {
        // Arrange & Act
        var spec = new OrderFilterSpec(null, null);

        // Assert
        spec.OrderByDescending.Should().NotBeNull();
        spec.OrderBy.Should().BeNull();
    }
}
