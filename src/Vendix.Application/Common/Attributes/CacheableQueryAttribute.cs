namespace Vendix.Application.Common.Attributes;

/// <summary>
/// Marks a query as cacheable with optional configuration.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class CacheableQueryAttribute : Attribute
{
    /// <summary>
    /// Custom cache key prefix. If not set, auto-generated from query type.
    /// </summary>
    public string? Key { get; set; }

    /// <summary>
    /// Cache expiration in minutes. Default is 5.
    /// </summary>
    public int ExpiryMinutes { get; set; } = 5;

    /// <summary>
    /// If true, bypasses cache for this request.
    /// </summary>
    public bool BypassCache { get; set; }
}
