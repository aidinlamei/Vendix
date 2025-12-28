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
        // Full includes for detail view - uses split query for better performance
        return await _context.Products
            .Include(p => p.Variants)
            .Include(p => p.Specifications)
            .Include(p => p.Images)
            .Include(p => p.Translations)
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        // Full includes for detail view - uses split query for better performance
        return await _context.Products
            .Include(p => p.Variants)
            .Include(p => p.Specifications)
            .Include(p => p.Images)
            .Include(p => p.Translations)
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Slug.Value == slug.ToLowerInvariant(), cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Product>> GetByCategoryAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        // Minimal includes for list view - only main image needed
        return await _context.Products
            .AsNoTracking()
            .Include(p => p.Images.Where(i => i.IsMain))
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Where(p => p.CategoryId == categoryId)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Product>> GetByBrandAsync(
        Guid brandId,
        CancellationToken cancellationToken = default)
    {
        // Minimal includes for list view - only main image needed
        return await _context.Products
            .AsNoTracking()
            .Include(p => p.Images.Where(i => i.IsMain))
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Where(p => p.BrandId == brandId)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> SearchAsync(
        string? searchTerm = null,
        Guid? categoryId = null,
        Guid? brandId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        // Build base query without heavy includes - optimized for list view
        var query = _context.Products
            .AsNoTracking()
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(p =>
                EF.Functions.ILike(p.Name, $"%{searchTerm}%") ||
                (p.Description != null && EF.Functions.ILike(p.Description, $"%{searchTerm}%")));
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

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination and minimal includes for list view
        // Only include main image for list display - NO variants, specifications, or translations
        var items = await query
            .Include(p => p.Images.Where(i => i.IsMain))
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        return (items, totalCount);
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
