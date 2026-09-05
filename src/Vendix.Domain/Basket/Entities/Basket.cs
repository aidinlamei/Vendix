using Vendix.Domain.Catalog.ValueObjects;
using Vendix.Domain.Common;

namespace Vendix.Domain.Basket.Entities;

/// <summary>
/// Represents a shopping basket owned by a single buyer (guest or, from Phase 5 onward,
/// an authenticated user).
/// </summary>
/// <remarks>
/// Basket is an aggregate root. One basket exists per <see cref="BuyerId"/> — see
/// <see cref="Repositories.IBasketRepository.GetByBuyerIdAsync"/>. A basket is not deleted
/// after checkout; it is emptied via <see cref="Clear"/> so the same buyer can keep shopping.
/// </remarks>
public class Basket : AggregateRoot
{
    private readonly List<BasketItem> _items = [];

    /// <summary>
    /// Gets the identifier of the buyer who owns this basket.
    /// </summary>
    public string BuyerId { get; private set; } = null!;

    /// <summary>
    /// Gets the line items in this basket.
    /// </summary>
    public IReadOnlyCollection<BasketItem> Items => _items.AsReadOnly();

    /// <summary>
    /// Required by EF Core for materialization.
    /// </summary>
    private Basket() { }

    /// <summary>
    /// Initializes a new, empty basket for the given buyer.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when buyerId is null or whitespace.</exception>
    public Basket(string buyerId)
    {
        if (string.IsNullOrWhiteSpace(buyerId))
        {
            throw new ArgumentException("Buyer id is required.", nameof(buyerId));
        }

        BuyerId = buyerId;
    }

    /// <summary>
    /// Adds a product to the basket. If the product is already present, the quantities are
    /// summed and the snapshot (name/slug/price/image) is refreshed to the latest values.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when quantity is not positive.</exception>
    public void AddItem(
        Guid productId,
        string productName,
        string productSlug,
        string sku,
        Money unitPrice,
        int quantity,
        string? imageUrl)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        var existing = _items.FirstOrDefault(i => i.ProductId == productId);
        if (existing is not null)
        {
            existing.IncreaseQuantity(quantity);
            existing.RefreshSnapshot(productName, productSlug, unitPrice, imageUrl);
            return;
        }

        _items.Add(new BasketItem(Id, productId, productName, productSlug, sku, unitPrice, quantity, imageUrl));
    }

    /// <summary>
    /// Sets the quantity of an existing item. A quantity of zero or less removes the item.
    /// Does nothing if the product is not in the basket.
    /// </summary>
    public void SetItemQuantity(Guid productId, int quantity)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item is null)
        {
            return;
        }

        if (quantity <= 0)
        {
            _items.Remove(item);
            return;
        }

        item.SetQuantity(quantity);
    }

    /// <summary>
    /// Removes a product from the basket entirely. Does nothing if not present.
    /// </summary>
    public void RemoveItem(Guid productId)
    {
        _items.RemoveAll(i => i.ProductId == productId);
    }

    /// <summary>
    /// Removes all items from the basket, leaving it empty (but not deleted).
    /// </summary>
    public void Clear()
    {
        _items.Clear();
    }
}
