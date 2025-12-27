using Vendix.Domain.Catalog.ValueObjects;
using Vendix.Domain.Common;

namespace Vendix.Domain.Catalog.Entities;

/// <summary>
/// Represents a variant of a product with its own SKU, price adjustment, and stock.
/// </summary>
/// <remarks>
/// Product variants are used for products that come in different sizes, colors, or configurations.
/// Each variant has its own SKU and inventory tracking but belongs to a parent product.
/// </remarks>
public class ProductVariant : BaseEntity
{
    /// <summary>
    /// Gets the ID of the parent product.
    /// </summary>
    public Guid ProductId { get; private set; }

    /// <summary>
    /// Gets the Stock Keeping Unit for this variant.
    /// </summary>
    public Sku Sku { get; private set; } = null!;

    /// <summary>
    /// Gets the variant name (e.g., "Large", "Blue", "64GB").
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Gets the price adjustment for this variant relative to the base product price.
    /// </summary>
    /// <remarks>
    /// This can be positive (variant costs more) or zero.
    /// The final price is calculated as: Product.Price + Variant.PriceAdjustment
    /// </remarks>
    public Money PriceAdjustment { get; private set; } = null!;

    /// <summary>
    /// Gets the current stock quantity for this variant.
    /// </summary>
    public int StockQuantity { get; private set; }

    /// <summary>
    /// Gets the parent product navigation property.
    /// </summary>
    public Product Product { get; private set; } = null!;

    /// <summary>
    /// Required by EF Core for materialization.
    /// </summary>
    private ProductVariant() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductVariant"/> class.
    /// </summary>
    /// <param name="productId">The ID of the parent product.</param>
    /// <param name="sku">The Stock Keeping Unit for this variant.</param>
    /// <param name="name">The variant name.</param>
    /// <param name="priceAdjustment">The price adjustment relative to the base price.</param>
    /// <param name="stockQuantity">The initial stock quantity. Must be non-negative.</param>
    /// <exception cref="ArgumentException">Thrown when name is null/whitespace or stockQuantity is negative.</exception>
    /// <exception cref="ArgumentNullException">Thrown when sku or priceAdjustment is null.</exception>
    public ProductVariant(
        Guid productId,
        Sku sku,
        string name,
        Money priceAdjustment,
        int stockQuantity = 0)
    {
        if (productId == Guid.Empty)
        {
            throw new ArgumentException("Product ID cannot be empty.", nameof(productId));
        }

        ProductId = productId;
        SetSku(sku);
        SetName(name);
        SetPriceAdjustment(priceAdjustment);
        SetStockQuantity(stockQuantity);
    }

    /// <summary>
    /// Updates the variant's SKU.
    /// </summary>
    /// <param name="sku">The new SKU.</param>
    /// <exception cref="ArgumentNullException">Thrown when sku is null.</exception>
    public void UpdateSku(Sku sku)
    {
        SetSku(sku);
    }

    /// <summary>
    /// Updates the variant name.
    /// </summary>
    /// <param name="name">The new variant name.</param>
    /// <exception cref="ArgumentException">Thrown when name is null or whitespace.</exception>
    public void UpdateName(string name)
    {
        SetName(name);
    }

    /// <summary>
    /// Updates the price adjustment for this variant.
    /// </summary>
    /// <param name="priceAdjustment">The new price adjustment.</param>
    /// <exception cref="ArgumentNullException">Thrown when priceAdjustment is null.</exception>
    public void UpdatePriceAdjustment(Money priceAdjustment)
    {
        SetPriceAdjustment(priceAdjustment);
    }

    /// <summary>
    /// Adds stock to this variant.
    /// </summary>
    /// <param name="quantity">The quantity to add. Must be positive.</param>
    /// <exception cref="ArgumentException">Thrown when quantity is not positive.</exception>
    public void AddStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity to add must be positive.", nameof(quantity));
        }

        StockQuantity += quantity;
    }

    /// <summary>
    /// Removes stock from this variant.
    /// </summary>
    /// <param name="quantity">The quantity to remove. Must be positive and not exceed current stock.</param>
    /// <exception cref="ArgumentException">Thrown when quantity is invalid.</exception>
    /// <exception cref="InvalidOperationException">Thrown when there is insufficient stock.</exception>
    public void RemoveStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity to remove must be positive.", nameof(quantity));
        }

        if (quantity > StockQuantity)
        {
            throw new InvalidOperationException($"Cannot remove {quantity} items. Only {StockQuantity} in stock.");
        }

        StockQuantity -= quantity;
    }

    /// <summary>
    /// Sets the stock quantity directly.
    /// </summary>
    /// <param name="quantity">The new stock quantity. Must be non-negative.</param>
    /// <exception cref="ArgumentException">Thrown when quantity is negative.</exception>
    public void SetStock(int quantity)
    {
        SetStockQuantity(quantity);
    }

    private void SetSku(Sku sku)
    {
        ArgumentNullException.ThrowIfNull(sku);
        Sku = sku;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Variant name is required.", nameof(name));
        }

        Name = name.Trim();
    }

    private void SetPriceAdjustment(Money priceAdjustment)
    {
        ArgumentNullException.ThrowIfNull(priceAdjustment);
        PriceAdjustment = priceAdjustment;
    }

    private void SetStockQuantity(int quantity)
    {
        if (quantity < 0)
        {
            throw new ArgumentException("Stock quantity cannot be negative.", nameof(quantity));
        }

        StockQuantity = quantity;
    }
}
