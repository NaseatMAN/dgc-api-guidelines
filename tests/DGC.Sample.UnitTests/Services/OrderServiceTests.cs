using DGC.Sample.Application.Dtos;
using DGC.Sample.Application.Interfaces.Persistence;
using DGC.Sample.Application.Interfaces.Repositories;
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
    private readonly IUnitOfWork _unitOfWork;
    private readonly OrderService _orderService;

    public OrderServiceTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _orderService = new OrderService(_orderRepository, _unitOfWork);
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
            Status = (int)OrderStatus.Draft, 
            TotalAmount = 100 
        };
        var updateRequest = new OrderUpdateRequest 
        { 
            CustomerName = "New Name", 
            OrderDateUtc = DateTime.UtcNow, 
            Status = OrderStatus.Fulfilled, 
            TotalAmount = 200 
        };

        _orderRepository.GetByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns(existingOrder);

        // Act
        var (response, created) = await _orderService.UpsertAsync(id, updateRequest, default);

        // Assert
        created.Should().BeFalse();
        response.CustomerName.Should().Be("New Name");
        response.TotalAmount.Should().Be(200);
        _orderRepository.Received(1).Update(Arg.Any<Order>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
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

        _orderRepository.GetByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns((Order?)null);

        // Act
        var (response, created) = await _orderService.UpsertAsync(id, updateRequest, default);

        // Assert
        created.Should().BeTrue();
        response.Id.Should().Be(id);
        response.CustomerName.Should().Be("New Name");
        _orderRepository.Received(1).Add(Arg.Any<Order>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllOrdersOrderedByOrderDateUtc()
    {
        // Arrange
        var orders = new[]
        {
            new Order { Id = Guid.NewGuid(), CustomerName = "B", OrderDateUtc = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc), Status = (int)OrderStatus.Submitted, TotalAmount = 20 },
            new Order { Id = Guid.NewGuid(), CustomerName = "A", OrderDateUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), Status = (int)OrderStatus.Draft, TotalAmount = 10 }
        }.AsQueryable();

        _orderRepository.QueryAsNoTracking().Returns(orders);

        // Act
        var result = await _orderService.GetAllAsync(default);

        // Assert
        result.Should().HaveCount(2);
        result.Select(order => order.CustomerName).Should().ContainInOrder("A", "B");
    }

    [Fact]
    public async Task GetPagingAsync_ShouldReturnPagedOrdersWithNextLink()
    {
        // Arrange
        var orders = new[]
        {
            new Order { Id = Guid.NewGuid(), CustomerName = "A", OrderDateUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), Status = (int)OrderStatus.Draft, TotalAmount = 10 },
            new Order { Id = Guid.NewGuid(), CustomerName = "B", OrderDateUtc = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc), Status = (int)OrderStatus.Submitted, TotalAmount = 20 },
            new Order { Id = Guid.NewGuid(), CustomerName = "C", OrderDateUtc = new DateTime(2026, 1, 3, 0, 0, 0, DateTimeKind.Utc), Status = (int)OrderStatus.Fulfilled, TotalAmount = 30 }
        }.AsQueryable();

        _orderRepository.QueryAsNoTracking().Returns(orders);

        // Act
        var result = await _orderService.GetPagingAsync(0, 2, default);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(3);
        result.Offset.Should().Be(0);
        result.Limit.Should().Be(2);
        result.NextLink.Should().BeEmpty();
        result.Items.Select(order => order.CustomerName).Should().ContainInOrder("A", "B");
    }

    [Fact]
    public async Task GetPagingAsync_WhenLastPage_ShouldReturnNullNextLink()
    {
        // Arrange
        var orders = new[]
        {
            new Order { Id = Guid.NewGuid(), CustomerName = "A", OrderDateUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), Status = (int)OrderStatus.Draft, TotalAmount = 10 },
            new Order { Id = Guid.NewGuid(), CustomerName = "B", OrderDateUtc = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc), Status = (int)OrderStatus.Submitted, TotalAmount = 20 }
        }.AsQueryable();

        _orderRepository.QueryAsNoTracking().Returns(orders);

        // Act
        var result = await _orderService.GetPagingAsync(0, 5, default);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.NextLink.Should().BeNull();
    }
}
