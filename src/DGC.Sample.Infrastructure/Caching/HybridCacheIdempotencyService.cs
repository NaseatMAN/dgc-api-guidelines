using DGC.Sample.Application.Dtos;
using DGC.Sample.Application.Interfaces;
using DGC.Sample.Application.Interfaces.Persistence;
using Microsoft.Extensions.Caching.Hybrid;

namespace DGC.Sample.Infrastructure.Caching;

public sealed class HybridCacheIdempotencyService(HybridCache cache) : IIdempotencyService
{
    private const string CacheKeyPrefix = "idempotency:";
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromHours(24);
    private static readonly TimeSpan ProcessingExpiration = TimeSpan.FromSeconds(30);
    private readonly HybridCache _cache = cache;

    public async Task<IdempotencyResult?> GetRequestAsync(string idempotencyKey, CancellationToken cancellationToken)
    {
        var cacheKey = $"{CacheKeyPrefix}{idempotencyKey}";

        return await _cache.GetOrCreateAsync(
            cacheKey,
            _ => ValueTask.FromResult<IdempotencyResult?>(null),
            cancellationToken: cancellationToken);
    }

    public async Task<bool> TryStartRequestAsync(string idempotencyKey, CancellationToken cancellationToken)
    {
        var cacheKey = $"{CacheKeyPrefix}{idempotencyKey}";
        
        // Try to get existing
        var existing = await GetRequestAsync(idempotencyKey, cancellationToken);
        if (existing != null)
        {
            return false;
        }

        // Set processing state
        var processingResult = new IdempotencyResult(0, string.Empty, IsProcessing: true);
        var options = new HybridCacheEntryOptions
        {
            Expiration = ProcessingExpiration,
            LocalCacheExpiration = ProcessingExpiration
        };

        await _cache.SetAsync(
            cacheKey,
            processingResult,
            options,
            cancellationToken: cancellationToken);

        return true;
    }

    public async Task SaveRequestAsync(string idempotencyKey, int statusCode, string responseBody, CancellationToken cancellationToken)
    {
        var cacheKey = $"{CacheKeyPrefix}{idempotencyKey}";
        var result = new IdempotencyResult(statusCode, responseBody, IsProcessing: false);

        var options = new HybridCacheEntryOptions
        {
            Expiration = DefaultExpiration,
            LocalCacheExpiration = DefaultExpiration
        };

        await _cache.SetAsync(
            cacheKey,
            result,
            options,
            cancellationToken: cancellationToken);
    }
}
