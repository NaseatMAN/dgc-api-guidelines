using DGC.Sample.Application.Common;
using DGC.Sample.Application.Dtos;
using DGC.Sample.Application.Interfaces.Persistence;
using Microsoft.Extensions.Caching.Hybrid;
using System.Collections.Concurrent;

namespace DGC.Sample.Infrastructure.Caching;

public sealed class HybridCacheIdempotencyService(HybridCache cache) : IIdempotencyService
{
    private const string CacheKeyPrefix = "idempotency:";
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromHours(24);
    private static readonly TimeSpan ProcessingExpiration = TimeSpan.FromSeconds(30);
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> Locks = new();
    private readonly HybridCache _cache = cache;

    public async Task<IdempotencyExecutionResult> TryStartRequestAsync(
        string idempotencyKey,
        string requestHash,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"{CacheKeyPrefix}{idempotencyKey}";
        var gate = Locks.GetOrAdd(cacheKey, static _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken);

        try
        {
            var existing = await _cache.GetAsync<IdempotencyResult?>(cacheKey, cancellationToken);
            if (existing is null)
            {
                var processingResult = new IdempotencyResult(0, string.Empty, requestHash, IsProcessing: true);
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

                return new IdempotencyExecutionResult(IdempotencyExecutionState.Started);
            }

            if (!string.Equals(existing.RequestHash, requestHash, StringComparison.Ordinal))
            {
                return new IdempotencyExecutionResult(IdempotencyExecutionState.RequestMismatch, existing);
            }

            return existing.IsProcessing
                ? new IdempotencyExecutionResult(IdempotencyExecutionState.Processing, existing)
                : new IdempotencyExecutionResult(IdempotencyExecutionState.Completed, existing);
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task SaveRequestAsync(
        string idempotencyKey,
        string requestHash,
        int statusCode,
        string responseBody,
        string contentType,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"{CacheKeyPrefix}{idempotencyKey}";
        var gate = Locks.GetOrAdd(cacheKey, static _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken);

        try
        {
            var existing = await _cache.GetAsync<IdempotencyResult?>(cacheKey, cancellationToken);
            if (existing is not null &&
                !string.Equals(existing.RequestHash, requestHash, StringComparison.Ordinal))
            {
                return;
            }

            var result = new IdempotencyResult(statusCode, responseBody, requestHash, contentType, IsProcessing: false);
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
        finally
        {
            gate.Release();
        }
    }

    public async Task ReleaseRequestAsync(string idempotencyKey, string requestHash, CancellationToken cancellationToken)
    {
        var cacheKey = $"{CacheKeyPrefix}{idempotencyKey}";
        var gate = Locks.GetOrAdd(cacheKey, static _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken);

        try
        {
            var existing = await _cache.GetAsync<IdempotencyResult?>(cacheKey, cancellationToken);
            if (existing is null ||
                !existing.IsProcessing ||
                !string.Equals(existing.RequestHash, requestHash, StringComparison.Ordinal))
            {
                return;
            }

            await _cache.RemoveAsync(cacheKey, cancellationToken);
        }
        finally
        {
            gate.Release();
        }
    }
}
