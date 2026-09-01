namespace Vendix.Web.Services;

/// <summary>
/// Represents a single product line in the guest shopping cart.
/// </summary>
public class CartItem
{
    /// <summary>
    /// Gets or sets the product identifier.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Gets or sets the product slug (used for linking back to the detail page).
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the product display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the product SKU.
    /// </summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unit price at the time the item was added to the cart.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets the price currency.
    /// </summary>
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Gets or sets the main product image URL.
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Gets or sets the quantity.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Gets the line total (price x quantity).
    /// </summary>
    public decimal LineTotal => Price * Quantity;
}
