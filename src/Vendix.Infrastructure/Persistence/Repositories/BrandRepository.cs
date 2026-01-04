using Microsoft.EntityFrameworkCore;
using Vendix.Domain.Catalog.Entities;
using Vendix.Domain.Catalog.Repositories;

namespace Vendix.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for managing Brand aggregates.
/// </summary>
/// <remarks>
/// Provides data access operations for brands including retrieval by slug.
/// </remarks>
public class BrandRepository : IBrandRepository
{
    private readonly VendixDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="BrandRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public BrandRepository(VendixDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<Brand?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Brands
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Brand?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var normalizedSlug = slug.ToLowerInvariant();
        return await _context.Brands
            .FirstOrDefaultAsync(b => EF.Property<string>(b, nameof(Brand.Slug)) == normalizedSlug, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Brand>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Brands
            .AsNoTracking()
            .OrderBy(b => b.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(Brand entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await _context.Brands.AddAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public void Update(Brand entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _context.Brands.Update(entity);
    }

    /// <inheritdoc />
    public void Delete(Brand entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _context.Brands.Remove(entity);
    }
}
