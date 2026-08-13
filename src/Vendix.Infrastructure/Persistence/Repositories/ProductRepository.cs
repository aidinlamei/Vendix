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
        var normalizedSlug = slug.ToLowerInvariant();
        // Load all products and filter in memory because EF Core can't translate Slug.Value directly
        var products = await _context.Products
            .Include(p => p.Variants)
            .Include(p => p.Specifications)
            .Include(p => p.Images)
            .Include(p => p.Translations)
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);
        
        return products.FirstOrDefault(p => p.Slug.Value.ToLowerInvariant() == normalizedSlug);
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
    public async Task<int> CountByBrandAsync(Guid brandId, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .AsNoTracking()
            .CountAsync(p => p.BrandId == brandId, cancellationToken);
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
            // Use parameterized query to prevent SQL injection
            // EF Core automatically parameterizes, but using explicit pattern is safer
            var searchPattern = $"%{searchTerm}%";
            query = query.Where(p =>
                EF.Functions.ILike(p.Name, searchPattern) ||
                (p.Description != null && EF.Functions.ILike(p.Description, searchPattern)));
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
        // Include variants for TotalStock calculation, all images for MainImageUrl selection
        // Note: Removed AsSplitQuery() to avoid ObjectDisposedException
        var items = await query
            .Include(p => p.Images) // Include all images, not just main ones
            .Include(p => p.Variants) // Include variants for TotalStock calculation
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
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
        
        // CRITICAL FIX: Detach any tracked entity with same ID
        var local = _context.Set<Product>()
            .Local
            .FirstOrDefault(e => e.Id == entity.Id);
            
        if (local != null)
        {
            _context.Entry(local).State = EntityState.Detached;
        }
        
        // Now attach and mark as modified
        _context.Entry(entity).State = EntityState.Modified;
    }

    /// <inheritdoc />
    public void Delete(Product entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _context.Products.Remove(entity);
    }

    /// <summary>
    /// Explicitly marks the Price owned entity as modified to ensure EF Core tracks changes.
    /// </summary>
    /// <param name="product">The product entity.</param>
    public void MarkPriceAsModified(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);
        
        var entry = _context.Entry(product);
        
        // Ensure the entity is being tracked and marked as modified
        if (entry.State == EntityState.Detached)
        {
            _context.Attach(product);
            entry = _context.Entry(product);
        }
        
        // Mark the entity as modified - this ensures EF Core will track all property changes
        entry.State = EntityState.Modified;
        
        // For owned entities, we need to explicitly mark the owned entity properties
        // Try to get the owned entity entry
        var priceReference = entry.Reference(p => p.Price);
        
        // If the owned entity is not loaded, load it
        if (!priceReference.IsLoaded)
        {
            priceReference.Load();
        }
        
        // Get the owned entity entry and mark its properties as modified
        var priceEntry = priceReference.TargetEntry;
        if (priceEntry != null)
        {
            // Mark both properties of the owned entity as modified
            priceEntry.Property(nameof(Vendix.Domain.Catalog.ValueObjects.Money.Amount)).IsModified = true;
            priceEntry.Property(nameof(Vendix.Domain.Catalog.ValueObjects.Money.Currency)).IsModified = true;
        }
    }
}
