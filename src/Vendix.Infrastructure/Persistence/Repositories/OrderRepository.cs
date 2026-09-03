using Microsoft.EntityFrameworkCore;
using Vendix.Domain.Ordering.Entities;
using Vendix.Domain.Ordering.Enums;
using Vendix.Domain.Ordering.Repositories;

namespace Vendix.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for managing Order aggregates.
/// </summary>
public class OrderRepository : IOrderRepository
{
    private readonly VendixDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderRepository"/> class.
    /// </summary>
    public OrderRepository(VendixDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Order>> GetByBuyerIdAsync(string buyerId, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Where(o => o.BuyerId == buyerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyList<Order> Items, int TotalCount)> SearchAsync(
        string? buyerId = null,
        OrderStatus? status = null,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Orders.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(buyerId))
        {
            query = query.Where(o => o.BuyerId == buyerId);
        }

        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <inheritdoc />
    public async Task AddAsync(Order entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await _context.Orders.AddAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public void Update(Order entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var local = _context.Set<Order>().Local.FirstOrDefault(o => o.Id == entity.Id);
        if (local is not null && !ReferenceEquals(local, entity))
        {
            _context.Entry(local).State = EntityState.Detached;
        }

        _context.Entry(entity).State = EntityState.Modified;
    }

    /// <inheritdoc />
    public void Delete(Order entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _context.Orders.Remove(entity);
    }
}
