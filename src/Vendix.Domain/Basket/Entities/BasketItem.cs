using Vendix.Domain.Catalog.ValueObjects;
using Vendix.Domain.Common;

namespace Vendix.Domain.Basket.Entities;

/// <summary>
/// Represents a single product line within a <see cref="Basket"/>.
/// </summary>
/// <remarks>
/// Product details (name, slug, SKU, price, image) are snapshotted at the time the item is
/// added so the basket keeps displaying correctly even if the product is later renamed,
/// repriced, or removed. The snapshot is refreshed whenever the quantity is increased via
/// <see cref="Basket.AddItem"/> re-adding the same product.
/// </remarks>
public class BasketItem : BaseEntity
{
    /// <summary>
    /// Gets the ID of the owning basket.
    /// </summary>
    public Guid BasketId { get; private set; }

    /// <summary>
    /// Gets the ID of the product this line represents.
    /// </summary>
    public Guid ProductId { get; private set; }

    /// <summary>
    /// Gets the product name snapshot.
    /// </summary>
    public string ProductName { get; private set; } = null!;

    /// <summary>
    /// Gets the product slug snapshot, used to link back to the product detail page.
    /// </summary>
    public string ProductSlug { get; private set; } = null!;

    /// <summary>
    /// Gets the product SKU snapshot.
    /// </summary>
    public string Sku { get; private set; } = null!;

    /// <summary>
    /// Gets the unit price snapshot.
    /// </summary>
    public Money UnitPrice { get; private set; } = null!;

    /// <summary>
    /// Gets the quantity of this product in the basket.
    /// </summary>
    public int Quantity { get; private set; }

    /// <summary>
    /// Gets the product image URL snapshot, if any.
    /// </summary>
    public string? ImageUrl { get; private set; }

    /// <summary>
    /// Gets the line total (unit price x quantity).
    /// </summary>
    public decimal LineTotal => UnitPrice.Amount * Quantity;

    /// <summary>
    /// Required by EF Core for materialization.
    /// </summary>
    private BasketItem() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BasketItem"/> class.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when quantity is not positive.</exception>
    /// <exception cref="ArgumentNullException">Thrown when unitPrice is null.</exception>
    public BasketItem(
        Guid basketId,
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

        BasketId = basketId;
        ProductId = productId;
        ProductName = productName;
        ProductSlug = productSlug;
        Sku = sku;
        UnitPrice = unitPrice ?? throw new ArgumentNullException(nameof(unitPrice));
        Quantity = quantity;
        ImageUrl = imageUrl;
    }

    /// <summary>
    /// Increases the quantity by the given positive amount.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when amount is not positive.</exception>
    public void IncreaseQuantity(int amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Amount must be greater than zero.", nameof(amount));
        }

        Quantity += amount;
    }

    /// <summary>
    /// Sets the quantity to an exact positive value.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when quantity is not positive.</exception>
    public void SetQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        Quantity = quantity;
    }

    /// <summary>
    /// Refreshes the price/name/image snapshot from the current product state.
    /// </summary>
    public void RefreshSnapshot(string productName, string productSlug, Money unitPrice, string? imageUrl)
    {
        ProductName = productName;
        ProductSlug = productSlug;
        UnitPrice = unitPrice ?? throw new ArgumentNullException(nameof(unitPrice));
        ImageUrl = imageUrl;
    }
}
