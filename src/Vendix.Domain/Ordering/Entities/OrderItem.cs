using Vendix.Domain.Common;

namespace Vendix.Domain.Ordering.Entities;

/// <summary>
/// Represents a single product line within a placed <see cref="Order"/>.
/// </summary>
/// <remarks>
/// Unlike <see cref="Basket.Entities.BasketItem"/>, this snapshot is permanent — an order line
/// must never change after checkout, even if the product is later repriced or deleted.
/// </remarks>
public class OrderItem : BaseEntity
{
    /// <summary>Gets the ID of the owning order.</summary>
    public Guid OrderId { get; private set; }

    /// <summary>Gets the ID of the product this line represents.</summary>
    public Guid ProductId { get; private set; }

    /// <summary>Gets the product name at the time of purchase.</summary>
    public string ProductName { get; private set; } = null!;

    /// <summary>Gets the product SKU at the time of purchase.</summary>
    public string Sku { get; private set; } = null!;

    /// <summary>Gets the unit price at the time of purchase.</summary>
    public decimal UnitPrice { get; private set; }

    /// <summary>Gets the purchased quantity.</summary>
    public int Quantity { get; private set; }

    /// <summary>Gets the product image URL at the time of purchase, if any.</summary>
    public string? ImageUrl { get; private set; }

    /// <summary>Gets the line total (unit price x quantity).</summary>
    public decimal LineTotal => UnitPrice * Quantity;

    /// <summary>
    /// Required by EF Core for materialization.
    /// </summary>
    private OrderItem() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderItem"/> class.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when quantity is not positive or unitPrice is negative.</exception>
    public OrderItem(Guid orderId, Guid productId, string productName, string sku, decimal unitPrice, int quantity, string? imageUrl)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        if (unitPrice < 0)
        {
            throw new ArgumentException("Unit price cannot be negative.", nameof(unitPrice));
        }

        OrderId = orderId;
        ProductId = productId;
        ProductName = productName;
        Sku = sku;
        UnitPrice = unitPrice;
        Quantity = quantity;
        ImageUrl = imageUrl;
    }
}
