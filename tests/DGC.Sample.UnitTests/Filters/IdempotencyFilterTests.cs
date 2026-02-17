using System.Text.Json;
using DGC.Sample.Api.Filters;
using DGC.Sample.Domain.Entities;
using DGC.Sample.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using NSubstitute;
using Xunit;

namespace DGC.Sample.UnitTests.Filters;

public sealed class IdempotencyFilterTests
{
    private readonly IIdempotencyService _idempotencyService;
    private readonly IdempotencyFilter _filter;

    public IdempotencyFilterTests()
    {
        _idempotencyService = Substitute.For<IIdempotencyService>();
        _filter = new IdempotencyFilter(_idempotencyService);
    }

    [Fact]
    public async Task OnActionExecutionAsync_WhenNoIdempotencyKey_ShouldCallNext()
    {
        // Arrange
        var context = CreateActionExecutingContext();
        var nextCalled = false;
        ActionExecutionDelegate next = () =>
        {
            nextCalled = true;
            return Task.FromResult(new ActionExecutedContext(context, new List<IFilterMetadata>(), Substitute.For<Controller>()));
        };

        // Act
        await _filter.OnActionExecutionAsync(context, next);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task OnActionExecutionAsync_WhenKeyExists_ShouldReturnCachedResult()
    {
        // Arrange
        var key = "test-key";
        var context = CreateActionExecutingContext(key);
        var cachedResponse = new IdempotentRequest
        {
            IdempotencyKey = key,
            RequestPath = "/test",
            StatusCode = 201,
            ResponseBody = "{\"id\": 1}"
        };

        _idempotencyService.GetRequestAsync(key, Arg.Any<CancellationToken>())
            .Returns(cachedResponse);

        ActionExecutionDelegate next = () => throw new Exception("Should not be called");

        // Act
        await _filter.OnActionExecutionAsync(context, next);

        // Assert
        context.Result.Should().BeOfType<ContentResult>();
        var result = (ContentResult)context.Result!;
        result.StatusCode.Should().Be(201);
        result.Content.Should().Be(cachedResponse.ResponseBody);
        context.HttpContext.Response.Headers["Idempotency-Key"].ToString().Should().Be(key);
        context.HttpContext.Response.Headers["Repeatability-Result"].ToString().Should().Be("accepted");
    }

    [Fact]
    public async Task OnActionExecutionAsync_WhenKeyNew_ShouldExecuteAndCache()
    {
        // Arrange
        var key = "new-key";
        var context = CreateActionExecutingContext(key);
        var responseValue = new { id = 1 };
        var actionExecutedContext = new ActionExecutedContext(context, new List<IFilterMetadata>(), Substitute.For<Controller>())
        {
            Result = new OkObjectResult(responseValue)
        };

        _idempotencyService.GetRequestAsync(key, Arg.Any<CancellationToken>())
            .Returns((IdempotentRequest?)null);

        ActionExecutionDelegate next = () => Task.FromResult(actionExecutedContext);

        // Act
        await _filter.OnActionExecutionAsync(context, next);

        // Assert
        await _idempotencyService.Received(1).SaveRequestAsync(
            key,
            Arg.Any<string>(),
            200,
            Arg.Is<string>(s => s.Contains("\"id\":1")),
            Arg.Any<CancellationToken>());
        
        context.HttpContext.Response.Headers["Idempotency-Key"].ToString().Should().Be(key);
    }

    private static ActionExecutingContext CreateActionExecutingContext(string? idempotencyKey = null)
    {
        var httpContext = new DefaultHttpContext();
        if (idempotencyKey != null)
        {
            httpContext.Request.Headers["Idempotency-Key"] = idempotencyKey;
        }

        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        return new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object?>(), Substitute.For<Controller>());
    }
}
