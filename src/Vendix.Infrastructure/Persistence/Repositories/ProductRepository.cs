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
        
        // Debug logging
        System.Diagnostics.Debug.WriteLine($"[Update Debug] Product Id: {entity.Id}, RowVersion: {(entity.RowVersion != null ? Convert.ToHexString(entity.RowVersion) : "null")}");
        
        // Check if entity is already tracked
        var trackedEntity = _context.Products.Local.FirstOrDefault(e => e.Id == entity.Id);
        if (trackedEntity != null)
        {
            System.Diagnostics.Debug.WriteLine($"[Update Debug] Found tracked entity, same reference: {ReferenceEquals(trackedEntity, entity)}");
            
            if (ReferenceEquals(trackedEntity, entity))
            {
                // Entity is already tracked and is the same instance
                // IMPORTANT: We need to reload OriginalValues from database to ensure RowVersion is correct
                var entry = _context.Entry(entity);
                
                // Reload OriginalValues from database using GetDatabaseValues
                // This is the safest way to reload OriginalValues without detaching
                try
                {
                    var databaseValues = entry.GetDatabaseValues();
                    if (databaseValues != null)
                    {
                        // Get RowVersion from database
                        var dbRowVersion = databaseValues.GetValue<byte[]>("RowVersion");
                        if (dbRowVersion != null)
                        {
                            // Set OriginalValue to the database value
                            entry.Property("RowVersion").OriginalValue = dbRowVersion;
                            System.Diagnostics.Debug.WriteLine($"[Update Debug] Reloaded Original RowVersion from DB: {Convert.ToHexString(dbRowVersion)}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // If GetDatabaseValues fails (e.g., entity doesn't exist in DB), log and continue
                    System.Diagnostics.Debug.WriteLine($"[Update Debug] Failed to reload OriginalValues: {ex.Message}");
                }
                
                // Ensure entity is marked as modified
                entry.State = EntityState.Modified;
                
                var originalValue = entry.Property("RowVersion").OriginalValue as byte[];
                System.Diagnostics.Debug.WriteLine($"[Update Debug] After reload OriginalValues, Original RowVersion: {(originalValue != null ? Convert.ToHexString(originalValue) : "null")}, Current RowVersion: {(entity.RowVersion != null ? Convert.ToHexString(entity.RowVersion) : "null")}");
                
                // Ensure all child entities are tracked
                if (entity.Images != null)
                {
                    foreach (var image in entity.Images)
                    {
                        var imageEntry = _context.Entry(image);
                        if (imageEntry.State == EntityState.Detached)
                        {
                            _context.ProductImages.Attach(image);
                            imageEntry.State = EntityState.Modified;
                        }
                        System.Diagnostics.Debug.WriteLine($"[Update Debug] Image {image.Id} state: {imageEntry.State}");
                    }
                }
                
                return;
            }
            else
            {
                // Detach the old tracked entity to avoid conflicts
                System.Diagnostics.Debug.WriteLine($"[Update Debug] Detaching old tracked entity");
                _context.Entry(trackedEntity).State = EntityState.Detached;
            }
        }
        
        // Attach and mark as modified - this will use entity's RowVersion as OriginalValues
        _context.Products.Attach(entity);
        var newEntry = _context.Entry(entity);
        newEntry.State = EntityState.Modified;
        
        var attachedOriginal = newEntry.Property("RowVersion").OriginalValue as byte[];
        var attachedCurrent = entity.RowVersion;
        System.Diagnostics.Debug.WriteLine($"[Update Debug] After Attach, Original RowVersion: {(attachedOriginal != null ? Convert.ToHexString(attachedOriginal) : "null")}, Current RowVersion: {(attachedCurrent != null ? Convert.ToHexString(attachedCurrent) : "null")}");
    }

    /// <inheritdoc />
    public void Delete(Product entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _context.Products.Remove(entity);
    }
}
