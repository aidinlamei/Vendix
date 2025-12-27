using Vendix.Domain.Catalog.Entities;
using Vendix.Domain.Common;

namespace Vendix.Domain.Catalog.Repositories;

/// <summary>
/// Repository interface for managing Brand entities.
/// </summary>
/// <remarks>
/// Extends the base repository with brand-specific query methods
/// for looking up brands by slug and retrieving all brands.
/// </remarks>
public interface IBrandRepository : IRepository<Brand>
{
    /// <summary>
    /// Gets a brand by its URL-friendly slug.
    /// </summary>
    /// <param name="slug">The brand slug.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The brand if found; otherwise, null.</returns>
    Task<Brand?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all brands.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A list of all brands.</returns>
    Task<IReadOnlyList<Brand>> GetAllAsync(CancellationToken cancellationToken = default);
}
