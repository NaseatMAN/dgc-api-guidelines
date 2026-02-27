using DGC.Sample.Infrastructure.Caching;
using FluentAssertions;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DGC.Sample.UnitTests.Services;

public sealed class HybridCacheIdempotencyServiceTests
{
    [Fact]
    public async Task GetRequestAsync_WhenKeyDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var cache = CreateHybridCache();
        var service = new HybridCacheIdempotencyService(cache);

        // Act
        var result = await service.GetRequestAsync(Guid.NewGuid().ToString("N"), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SaveRequestAsync_ThenGetRequestAsync_ShouldReturnSavedResponse()
    {
        // Arrange
        var cache = CreateHybridCache();
        var service = new HybridCacheIdempotencyService(cache);
        var idempotencyKey = Guid.NewGuid().ToString("N");

        // Act
        await service.SaveRequestAsync(idempotencyKey, 201, "{\"id\":1}", CancellationToken.None);
        var result = await service.GetRequestAsync(idempotencyKey, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(201);
        result.ResponseBody.Should().Be("{\"id\":1}");
        result.IsProcessing.Should().BeFalse();
    }

    private static HybridCache CreateHybridCache()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHybridCache();

        return services.BuildServiceProvider().GetRequiredService<HybridCache>();
    }
}