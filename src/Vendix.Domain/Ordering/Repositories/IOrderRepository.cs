using Vendix.Domain.Common;
using Vendix.Domain.Ordering.Enums;

namespace Vendix.Domain.Ordering.Repositories;

/// <summary>
/// Repository interface for managing Order aggregates.
/// </summary>
public interface IOrderRepository : IRepository<Entities.Order>
{
    /// <summary>
    /// Gets all orders placed by the given buyer, most recent first.
    /// </summary>
    Task<IReadOnlyList<Entities.Order>> GetByBuyerIdAsync(string buyerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches orders with optional buyer and status filters, paginated, most recent first.
    /// </summary>
    Task<(IReadOnlyList<Entities.Order> Items, int TotalCount)> SearchAsync(
        string? buyerId = null,
        OrderStatus? status = null,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);
}
