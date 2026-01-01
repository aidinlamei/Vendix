using Vendix.Domain.Catalog.Entities;
using Vendix.Domain.Common;

namespace Vendix.Domain.Catalog.Repositories;

/// <summary>
/// Repository interface for managing Product aggregates.
/// </summary>
/// <remarks>
/// Extends the base repository with product-specific query methods
/// for searching, filtering by category, and looking up by slug.
/// </remarks>
public interface IProductRepository : IRepository<Product>
{
    /// <summary>
    /// Gets a product by its URL-friendly slug.
    /// </summary>
    /// <param name="slug">The product slug.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The product if found; otherwise, null.</returns>
    Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets products belonging to a specific category.
    /// </summary>
    /// <param name="categoryId">The category ID to filter by.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A list of products in the specified category.</returns>
    Task<IReadOnlyList<Product>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets products belonging to a specific brand.
    /// </summary>
    /// <param name="brandId">The brand ID to filter by.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A list of products for the specified brand.</returns>
    Task<IReadOnlyList<Product>> GetByBrandAsync(Guid brandId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of products belonging to a specific brand.
    /// </summary>
    /// <param name="brandId">The brand ID to filter by.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The number of products for the specified brand.</returns>
    Task<int> CountByBrandAsync(Guid brandId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for products matching the specified criteria.
    /// </summary>
    /// <param name="searchTerm">Optional search term to match against name and description.</param>
    /// <param name="categoryId">Optional category ID to filter by.</param>
    /// <param name="brandId">Optional brand ID to filter by.</param>
    /// <param name="minPrice">Optional minimum price filter.</param>
    /// <param name="maxPrice">Optional maximum price filter.</param>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A tuple containing the list of products matching the search criteria and the total count.</returns>
    Task<(IReadOnlyList<Product> Items, int TotalCount)> SearchAsync(
        string? searchTerm = null,
        Guid? categoryId = null,
        Guid? brandId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);
}
