using Microsoft.EntityFrameworkCore;
using Vendix.Domain.Basket.Entities;
using Vendix.Domain.Basket.Repositories;

namespace Vendix.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for managing Basket aggregates.
/// </summary>
public class BasketRepository : IBasketRepository
{
    private readonly VendixDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="BasketRepository"/> class.
    /// </summary>
    public BasketRepository(VendixDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<Basket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Baskets
            .Include(b => b.Items)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Basket?> GetByBuyerIdAsync(string buyerId, CancellationToken cancellationToken = default)
    {
        return await _context.Baskets
            .Include(b => b.Items)
            .FirstOrDefaultAsync(b => b.BuyerId == buyerId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(Basket entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await _context.Baskets.AddAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public void Update(Basket entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var local = _context.Set<Basket>().Local.FirstOrDefault(b => b.Id == entity.Id);
        if (local is not null && !ReferenceEquals(local, entity))
        {
            _context.Entry(local).State = EntityState.Detached;
        }

        _context.Entry(entity).State = EntityState.Modified;
    }

    /// <inheritdoc />
    public void Delete(Basket entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _context.Baskets.Remove(entity);
    }
}
