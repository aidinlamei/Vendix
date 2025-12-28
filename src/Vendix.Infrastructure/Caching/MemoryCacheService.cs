using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Vendix.Application.Common.Interfaces;

namespace Vendix.Infrastructure.Caching;

/// <summary>
/// In-memory implementation of the cache service using IMemoryCache.
/// </summary>
/// <remarks>
/// This implementation tracks all cache keys to support prefix-based removal.
/// For production, consider upgrading to Redis-based implementation for distributed caching.
/// </remarks>
public sealed class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ConcurrentDictionary<string, byte> _cacheKeys = new();
    private static readonly TimeSpan DefaultExpiry = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryCacheService"/> class.
    /// </summary>
    /// <param name="memoryCache">The memory cache instance.</param>
    public MemoryCacheService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    }

    /// <inheritdoc />
    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var value = _memoryCache.TryGetValue<T>(key, out var cachedValue) ? cachedValue : default;
        return Task.FromResult(value);
    }

    /// <inheritdoc />
    public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var cacheExpiry = expiry ?? DefaultExpiry;
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(cacheExpiry)
            .RegisterPostEvictionCallback((evictedKey, evictedValue, reason, state) =>
            {
                // Remove from tracked keys when evicted
                _cacheKeys.TryRemove(evictedKey.ToString()!, out _);
            });

        _memoryCache.Set(key, value, cacheEntryOptions);
        _cacheKeys.TryAdd(key, 0);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        _memoryCache.Remove(key);
        _cacheKeys.TryRemove(key, out _);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prefix);

        var keysToRemove = _cacheKeys.Keys
            .Where(key => key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var key in keysToRemove)
        {
            _memoryCache.Remove(key);
            _cacheKeys.TryRemove(key, out _);
        }

        return Task.CompletedTask;
    }
}
