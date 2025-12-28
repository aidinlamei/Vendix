using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Vendix.Application.Common.Interfaces;

namespace Vendix.Infrastructure.Caching;

/// <summary>
/// Memory cache implementation of <see cref="ICacheService"/>.
/// </summary>
/// <remarks>
/// Uses IMemoryCache for in-process caching. This is suitable for single-server
/// deployments. For distributed scenarios, consider using Redis.
/// </remarks>
public sealed class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<MemoryCacheService> _logger;
    private readonly CacheSettings _settings;
    private readonly ConcurrentDictionary<string, byte> _keys = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryCacheService"/> class.
    /// </summary>
    /// <param name="cache">The memory cache instance.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="settings">The cache settings.</param>
    public MemoryCacheService(
        IMemoryCache cache,
        ILogger<MemoryCacheService> logger,
        IOptions<CacheSettings> settings)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings?.Value ?? new CacheSettings();
    }

    /// <inheritdoc />
    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        if (_cache.TryGetValue(key, out T? value))
        {
            _logger.LogDebug("Cache hit for key: {Key}", key);
            return Task.FromResult(value);
        }

        _logger.LogDebug("Cache miss for key: {Key}", key);
        return Task.FromResult<T?>(default);
    }

    /// <inheritdoc />
    public Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiry = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiry ?? TimeSpan.FromMinutes(_settings.DefaultExpirationMinutes)
        };

        _cache.Set(key, value, cacheOptions);
        _keys.TryAdd(key, 0);

        _logger.LogDebug(
            "Cached value for key: {Key} with expiry: {Expiry}",
            key,
            cacheOptions.AbsoluteExpirationRelativeToNow);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        _cache.Remove(key);
        _keys.TryRemove(key, out _);

        _logger.LogDebug("Removed cache entry for key: {Key}", key);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prefix);

        var keysToRemove = _keys.Keys
            .Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var key in keysToRemove)
        {
            _cache.Remove(key);
            _keys.TryRemove(key, out _);
        }

        _logger.LogDebug(
            "Removed {Count} cache entries with prefix: {Prefix}",
            keysToRemove.Count,
            prefix);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<T?> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? expiry = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(factory);

        var cached = await GetAsync<T>(key, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        var value = await factory(cancellationToken);
        if (value is not null)
        {
            await SetAsync(key, value, expiry, cancellationToken);
        }

        return value;
    }
}

/// <summary>
/// Configuration settings for the cache service.
/// </summary>
public sealed class CacheSettings
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Cache";

    /// <summary>
    /// Gets or sets the default expiration time in minutes.
    /// </summary>
    public int DefaultExpirationMinutes { get; set; } = 5;

    /// <summary>
    /// Gets or sets the maximum number of items to store in the cache.
    /// </summary>
    public int MaxItems { get; set; } = 1000;
}
