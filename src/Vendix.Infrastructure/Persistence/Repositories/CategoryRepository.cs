using Microsoft.EntityFrameworkCore;
using Vendix.Domain.Catalog.Entities;
using Vendix.Domain.Catalog.Repositories;

namespace Vendix.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for managing Category aggregates.
/// </summary>
/// <remarks>
/// Provides data access operations for categories including hierarchical navigation
/// and retrieval with translations.
/// </remarks>
public class CategoryRepository : ICategoryRepository
{
    private readonly VendixDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="CategoryRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public CategoryRepository(VendixDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .Include(c => c.Translations)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .Include(c => c.Translations)
            .FirstOrDefaultAsync(c => c.Slug.Value == slug.ToLowerInvariant(), cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Category>> GetRootCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .AsNoTracking()
            .Include(c => c.Translations)
            .Where(c => c.ParentCategoryId == null)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Category?> GetWithChildrenAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .Include(c => c.Translations)
            .Include(c => c.SubCategories)
                .ThenInclude(sc => sc.Translations)
            .FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(Category entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await _context.Categories.AddAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public void Update(Category entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _context.Categories.Update(entity);
    }

    /// <inheritdoc />
    public void Delete(Category entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _context.Categories.Remove(entity);
    }
}
