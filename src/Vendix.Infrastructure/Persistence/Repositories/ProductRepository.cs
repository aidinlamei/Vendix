using Microsoft.EntityFrameworkCore;
using Vendix.Domain.Catalog.Entities;
using Vendix.Domain.Catalog.Repositories;

namespace Vendix.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for managing Product aggregates.
/// </summary>
/// <remarks>
/// Provides data access operations for products including search, filtering,
/// and retrieval with related entities (variants, specifications, images, translations).
/// </remarks>
public class ProductRepository : IProductRepository
{
    private readonly VendixDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public ProductRepository(VendixDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.Variants)
            .Include(p => p.Specifications)
            .Include(p => p.Images)
            .Include(p => p.Translations)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.Variants)
            .Include(p => p.Specifications)
            .Include(p => p.Images)
            .Include(p => p.Translations)
            .FirstOrDefaultAsync(p => p.Slug.Value == slug.ToLowerInvariant(), cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Product>> GetByCategoryAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .AsNoTracking()
            .Include(p => p.Variants)
            .Include(p => p.Images)
            .Include(p => p.Translations)
            .Where(p => p.CategoryId == categoryId)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Product>> SearchAsync(
        string? searchTerm = null,
        Guid? categoryId = null,
        Guid? brandId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Products
            .AsNoTracking()
            .Include(p => p.Variants)
            .Include(p => p.Images)
            .Include(p => p.Translations)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLowerInvariant();
            query = query.Where(p =>
                p.Name.ToLower().Contains(term) ||
                (p.Description != null && p.Description.ToLower().Contains(term)) ||
                p.Translations.Any(t => t.Title.ToLower().Contains(term)));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        if (brandId.HasValue)
        {
            query = query.Where(p => p.BrandId == brandId.Value);
        }

        if (minPrice.HasValue)
        {
            query = query.Where(p => p.Price.Amount >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.Price.Amount <= maxPrice.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(Product entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await _context.Products.AddAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public void Update(Product entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _context.Products.Update(entity);
    }

    /// <inheritdoc />
    public void Delete(Product entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _context.Products.Remove(entity);
    }
}
