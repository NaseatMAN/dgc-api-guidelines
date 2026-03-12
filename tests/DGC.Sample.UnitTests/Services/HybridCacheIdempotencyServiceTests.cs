using DGC.Sample.Application.Dtos;
using DGC.Sample.Infrastructure.Caching;
using FluentAssertions;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DGC.Sample.UnitTests.Services;

public sealed class HybridCacheIdempotencyServiceTests
{
    [Fact]
    public async Task TryStartRequestAsync_WhenKeyDoesNotExist_ShouldReserveKey()
    {
        var cache = CreateHybridCache();
        var service = new HybridCacheIdempotencyService(cache);
        var idempotencyKey = Guid.NewGuid().ToString("N");
        var requestHash = "hash-1";

        var result = await service.TryStartRequestAsync(idempotencyKey, requestHash, CancellationToken.None);

        result.State.Should().Be(IdempotencyExecutionState.Started);
        result.CachedResponse.Should().BeNull();
    }

    [Fact]
    public async Task SaveRequestAsync_ThenTryStartRequestAsync_ShouldReturnSavedResponse()
    {
        var cache = CreateHybridCache();
        var service = new HybridCacheIdempotencyService(cache);
        var idempotencyKey = Guid.NewGuid().ToString("N");
        var requestHash = "hash-1";

        await service.TryStartRequestAsync(idempotencyKey, requestHash, CancellationToken.None);
        await service.SaveRequestAsync(idempotencyKey, requestHash, 201, "{\"id\":1}", "application/json", CancellationToken.None);
        var result = await service.TryStartRequestAsync(idempotencyKey, requestHash, CancellationToken.None);

        result.State.Should().Be(IdempotencyExecutionState.Completed);
        result.CachedResponse.Should().NotBeNull();
        result.CachedResponse!.StatusCode.Should().Be(201);
        result.CachedResponse.ResponseBody.Should().Be("{\"id\":1}");
        result.CachedResponse.IsProcessing.Should().BeFalse();
    }

    [Fact]
    public async Task TryStartRequestAsync_WhenSameKeyUsesDifferentHash_ShouldReturnMismatch()
    {
        var cache = CreateHybridCache();
        var service = new HybridCacheIdempotencyService(cache);
        var idempotencyKey = Guid.NewGuid().ToString("N");

        await service.TryStartRequestAsync(idempotencyKey, "hash-1", CancellationToken.None);

        var result = await service.TryStartRequestAsync(idempotencyKey, "hash-2", CancellationToken.None);

        result.State.Should().Be(IdempotencyExecutionState.RequestMismatch);
    }

    [Fact]
    public async Task ReleaseRequestAsync_ShouldAllowRetryAfterFailure()
    {
        var cache = CreateHybridCache();
        var service = new HybridCacheIdempotencyService(cache);
        var idempotencyKey = Guid.NewGuid().ToString("N");
        const string requestHash = "hash-1";

        await service.TryStartRequestAsync(idempotencyKey, requestHash, CancellationToken.None);
        await service.ReleaseRequestAsync(idempotencyKey, requestHash, CancellationToken.None);

        var result = await service.TryStartRequestAsync(idempotencyKey, requestHash, CancellationToken.None);

        result.State.Should().Be(IdempotencyExecutionState.Started);
    }

    private static HybridCache CreateHybridCache()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHybridCache();

        return services.BuildServiceProvider().GetRequiredService<HybridCache>();
    }
}
