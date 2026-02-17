using DGC.Sample.Application.Dtos;
using DGC.Sample.Application.Interfaces;
using DGC.Sample.Application.Services;
using DGC.Sample.Domain.Entities;
using DGC.Sample.Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace DGC.Sample.UnitTests.Services;

public sealed class OrderServiceTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly OrderService _orderService;

    public OrderServiceTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();
        _orderService = new OrderService(_orderRepository);
    }

    [Fact]
    public async Task UpsertAsync_WhenOrderExists_ShouldUpdateAndReturnCreatedFalse()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingOrder = new Order 
        { 
            Id = id, 
            CustomerName = "Old Name", 
            OrderDateUtc = DateTime.UtcNow, 
            Status = OrderStatus.Draft, 
            TotalAmount = 100 
        };
        var updateRequest = new OrderUpdateRequest 
        { 
            CustomerName = "New Name", 
            OrderDateUtc = DateTime.UtcNow, 
            Status = OrderStatus.Fulfilled, 
            TotalAmount = 200 
        };

        _orderRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(existingOrder);

        // Act
        var (response, created) = await _orderService.UpsertAsync(id, updateRequest, default);

        // Assert
        created.Should().BeFalse();
        response.CustomerName.Should().Be("New Name");
        response.TotalAmount.Should().Be(200);
        await _orderRepository.Received(1).UpdateAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpsertAsync_WhenOrderDoesNotExist_ShouldCreateAndReturnCreatedTrue()
    {
        // Arrange
        var id = Guid.NewGuid();
        var updateRequest = new OrderUpdateRequest 
        { 
            CustomerName = "New Name", 
            OrderDateUtc = DateTime.UtcNow, 
            Status = OrderStatus.Fulfilled, 
            TotalAmount = 200 
        };

        _orderRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Order?)null);

        // Act
        var (response, created) = await _orderService.UpsertAsync(id, updateRequest, default);

        // Assert
        created.Should().BeTrue();
        response.Id.Should().Be(id);
        response.CustomerName.Should().Be("New Name");
        await _orderRepository.Received(1).AddAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }
}
