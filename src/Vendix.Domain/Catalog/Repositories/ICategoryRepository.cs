using Vendix.Domain.Catalog.Entities;
using Vendix.Domain.Common;

namespace Vendix.Domain.Catalog.Repositories;

/// <summary>
/// Repository interface for managing Category aggregates.
/// </summary>
/// <remarks>
/// Extends the base repository with category-specific query methods
/// for navigating the category hierarchy and looking up by slug.
/// </remarks>
public interface ICategoryRepository : IRepository<Category>
{
    /// <summary>
    /// Gets a category by its URL-friendly slug.
    /// </summary>
    /// <param name="slug">The category slug.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The category if found; otherwise, null.</returns>
    Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all root categories (categories without a parent).
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A list of root categories.</returns>
    Task<IReadOnlyList<Category>> GetRootCategoriesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a category with all its child categories loaded.
    /// </summary>
    /// <param name="categoryId">The parent category ID.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The category with children if found; otherwise, null.</returns>
    Task<Category?> GetWithChildrenAsync(Guid categoryId, CancellationToken cancellationToken = default);
}
