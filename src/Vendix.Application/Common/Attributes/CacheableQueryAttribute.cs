namespace Vendix.Application.Common.Attributes;

/// <summary>
/// Marks a query as cacheable with specified cache key and expiry.
/// </summary>
/// <remarks>
/// Apply this attribute to query classes (e.g., GetProductsQuery) to enable caching.
/// The CachingBehavior pipeline will intercept queries with this attribute and cache their results.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class CacheableQueryAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the cache key for this query.
    /// </summary>
    /// <remarks>
    /// Use constants from CacheKeys class for consistency.
    /// For parameterized queries, the behavior will append parameter values to this key.
    /// </remarks>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the cache expiry time in minutes.
    /// </summary>
    /// <remarks>
    /// Default is 5 minutes. Adjust based on data volatility:
    /// - Products: 5 minutes
    /// - Categories: 30 minutes
    /// - Settings: 60 minutes
    /// </remarks>
    public int ExpiryMinutes { get; set; } = 5;
}
