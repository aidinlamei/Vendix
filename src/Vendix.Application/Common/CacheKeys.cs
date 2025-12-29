namespace Vendix.Application.Common;

/// <summary>
/// Centralized cache key prefixes for consistency.
/// </summary>
public static class CacheKeys
{
    /// <summary>
    /// Cache key prefix for products.
    /// </summary>
    public const string Products = "products";

    /// <summary>
    /// Cache key prefix for categories.
    /// </summary>
    public const string Categories = "categories";

    /// <summary>
    /// Cache key prefix for brands.
    /// </summary>
    public const string Brands = "brands";

    /// <summary>
    /// Time-to-live constants (in minutes).
    /// </summary>
    public static class Ttl
    {
        /// <summary>
        /// TTL for product queries (5 minutes).
        /// </summary>
        public const int Products = 5;

        /// <summary>
        /// TTL for category queries (30 minutes).
        /// </summary>
        public const int Categories = 30;

        /// <summary>
        /// TTL for brand queries (30 minutes).
        /// </summary>
        public const int Brands = 30;
    }
}

