namespace Vendix.Infrastructure.Caching;

/// <summary>
/// Contains cache key constants and helper methods for generating cache keys.
/// </summary>
/// <remarks>
/// Using constants for cache keys ensures consistency and prevents typos.
/// Key format follows the pattern: {Domain}:{Entity}:{Identifier}
/// </remarks>
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
    /// Default cache expiration in minutes for product data.
    /// </summary>
    public const int ProductsCacheMinutes = 5;

    /// <summary>
    /// Default cache expiration in minutes for category data.
    /// </summary>
    public const int CategoriesCacheMinutes = 30;

    /// <summary>
    /// Default cache expiration in minutes for brand data.
    /// </summary>
    public const int BrandsCacheMinutes = 30;

    /// <summary>
    /// Generates a cache key for a specific product by ID.
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <returns>The cache key.</returns>
    public static string ProductById(Guid productId) => $"{Products}:id:{productId}";

    /// <summary>
    /// Generates a cache key for a specific product by slug.
    /// </summary>
    /// <param name="slug">The product slug.</param>
    /// <returns>The cache key.</returns>
    public static string ProductBySlug(string slug) => $"{Products}:slug:{slug.ToLowerInvariant()}";

    /// <summary>
    /// Generates a cache key for a product search query.
    /// </summary>
    /// <param name="searchTerm">The search term.</param>
    /// <param name="categoryId">Optional category filter.</param>
    /// <param name="brandId">Optional brand filter.</param>
    /// <param name="minPrice">Optional min price filter.</param>
    /// <param name="maxPrice">Optional max price filter.</param>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <returns>The cache key.</returns>
    public static string ProductSearch(
        string? searchTerm,
        Guid? categoryId,
        Guid? brandId,
        decimal? minPrice,
        decimal? maxPrice,
        int pageNumber,
        int pageSize)
    {
        var key = $"{Products}:search:" +
            $"q:{searchTerm ?? "null"}:" +
            $"cat:{categoryId?.ToString() ?? "null"}:" +
            $"brand:{brandId?.ToString() ?? "null"}:" +
            $"min:{minPrice?.ToString("F2") ?? "null"}:" +
            $"max:{maxPrice?.ToString("F2") ?? "null"}:" +
            $"p:{pageNumber}:s:{pageSize}";

        return key;
    }

    /// <summary>
    /// Generates a cache key for a specific category by ID.
    /// </summary>
    /// <param name="categoryId">The category ID.</param>
    /// <returns>The cache key.</returns>
    public static string CategoryById(Guid categoryId) => $"{Categories}:id:{categoryId}";

    /// <summary>
    /// Generates a cache key for a specific category by slug.
    /// </summary>
    /// <param name="slug">The category slug.</param>
    /// <returns>The cache key.</returns>
    public static string CategoryBySlug(string slug) => $"{Categories}:slug:{slug.ToLowerInvariant()}";

    /// <summary>
    /// Cache key for root categories list.
    /// </summary>
    public const string RootCategories = $"{Categories}:root";

    /// <summary>
    /// Generates a cache key for a specific brand by ID.
    /// </summary>
    /// <param name="brandId">The brand ID.</param>
    /// <returns>The cache key.</returns>
    public static string BrandById(Guid brandId) => $"{Brands}:id:{brandId}";

    /// <summary>
    /// Generates a cache key for a specific brand by slug.
    /// </summary>
    /// <param name="slug">The brand slug.</param>
    /// <returns>The cache key.</returns>
    public static string BrandBySlug(string slug) => $"{Brands}:slug:{slug.ToLowerInvariant()}";

    /// <summary>
    /// Cache key for all brands list.
    /// </summary>
    public const string AllBrands = $"{Brands}:all";
}
