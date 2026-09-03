using Vendix.Domain.Basket.Entities;
using Vendix.Domain.Common;

namespace Vendix.Domain.Basket.Repositories;

/// <summary>
/// Repository interface for managing Basket aggregates.
/// </summary>
public interface IBasketRepository : IRepository<Entities.Basket>
{
    /// <summary>
    /// Gets the basket owned by the given buyer, or null if they don't have one yet.
    /// </summary>
    Task<Entities.Basket?> GetByBuyerIdAsync(string buyerId, CancellationToken cancellationToken = default);
}
