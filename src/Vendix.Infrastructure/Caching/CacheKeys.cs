namespace Vendix.Infrastructure.Caching;

/// <summary>
/// Provides constants for cache keys used throughout the application.
/// </summary>
/// <remarks>
/// Using consistent cache keys helps with invalidation and prevents key collisions.
/// All keys are prefixed by domain entity for easy prefix-based removal.
/// </remarks>
public static class CacheKeys
{
    /// <summary>
    /// Cache key prefix for all product-related cache entries.
    /// </summary>
    public const string ProductsPrefix = "products:";

    /// <summary>
    /// Cache key for all products list.
    /// </summary>
    public const string AllProducts = "products:all";

    /// <summary>
    /// Cache key pattern for a specific product by ID. Use string.Format to create actual key.
    /// </summary>
    public const string ProductById = "products:id:{0}";

    /// <summary>
    /// Cache key pattern for a specific product by slug. Use string.Format to create actual key.
    /// </summary>
    public const string ProductBySlug = "products:slug:{0}";

    /// <summary>
    /// Cache key prefix for all category-related cache entries.
    /// </summary>
    public const string CategoriesPrefix = "categories:";

    /// <summary>
    /// Cache key for all categories list.
    /// </summary>
    public const string AllCategories = "categories:all";

    /// <summary>
    /// Cache key pattern for a specific category by ID. Use string.Format to create actual key.
    /// </summary>
    public const string CategoryById = "categories:id:{0}";

    /// <summary>
    /// Cache key prefix for all brand-related cache entries.
    /// </summary>
    public const string BrandsPrefix = "brands:";

    /// <summary>
    /// Cache key for all brands list.
    /// </summary>
    public const string AllBrands = "brands:all";

    /// <summary>
    /// Cache key pattern for a specific brand by ID. Use string.Format to create actual key.
    /// </summary>
    public const string BrandById = "brands:id:{0}";

    /// <summary>
    /// Cache key for store settings.
    /// </summary>
    public const string StoreSettings = "settings:store";

    /// <summary>
    /// Helper method to create a cache key for a product by ID.
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <returns>The formatted cache key.</returns>
    public static string GetProductByIdKey(Guid productId) => string.Format(ProductById, productId);

    /// <summary>
    /// Helper method to create a cache key for a product by slug.
    /// </summary>
    /// <param name="slug">The product slug.</param>
    /// <returns>The formatted cache key.</returns>
    public static string GetProductBySlugKey(string slug) => string.Format(ProductBySlug, slug);

    /// <summary>
    /// Helper method to create a cache key for a category by ID.
    /// </summary>
    /// <param name="categoryId">The category ID.</param>
    /// <returns>The formatted cache key.</returns>
    public static string GetCategoryByIdKey(Guid categoryId) => string.Format(CategoryById, categoryId);

    /// <summary>
    /// Helper method to create a cache key for a brand by ID.
    /// </summary>
    /// <param name="brandId">The brand ID.</param>
    /// <returns>The formatted cache key.</returns>
    public static string GetBrandByIdKey(Guid brandId) => string.Format(BrandById, brandId);
}
