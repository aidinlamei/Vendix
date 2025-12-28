namespace Vendix.Application.Common.Interfaces;

/// <summary>
/// Provides caching functionality for application data.
/// </summary>
/// <remarks>
/// Implementations should use IMemoryCache for Phase 1 and can be upgraded to Redis for production.
/// Cache keys should be prefixed with constants from CacheKeys class for consistency.
/// </remarks>
public interface ICacheService
{
    /// <summary>
    /// Gets a cached value by its key.
    /// </summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The cached value if found; otherwise, null.</returns>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a value in the cache with an optional expiry time.
    /// </summary>
    /// <typeparam name="T">The type of the value to cache.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    /// <param name="expiry">Optional expiry time. If null, uses default expiry from configuration.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a cached value by its key.
    /// </summary>
    /// <param name="key">The cache key to remove.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all cached values that start with the specified prefix.
    /// </summary>
    /// <param name="prefix">The key prefix to match.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <remarks>
    /// This is useful for invalidating groups of related cache entries.
    /// For example, removing all product cache entries when a product is updated.
    /// </remarks>
    Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);
}
