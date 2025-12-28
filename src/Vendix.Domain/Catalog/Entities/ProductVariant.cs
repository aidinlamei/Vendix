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
    /// Gets the price adjustment amount for this variant relative to the base product price.
    /// </summary>
    /// <remarks>
    /// This can be positive (variant costs more), negative (discount), or zero.
    /// The final price is calculated as: Product.Price + Variant.PriceAdjustmentAmount
    /// </remarks>
    public decimal PriceAdjustmentAmount { get; private set; }

    /// <summary>
    /// Gets the currency code for the price adjustment (e.g., "USD", "EUR").
    /// </summary>
    public string PriceAdjustmentCurrency { get; private set; } = null!;

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
    /// <param name="priceAdjustmentAmount">The price adjustment amount (can be negative for discounts).</param>
    /// <param name="priceAdjustmentCurrency">The currency code for the price adjustment.</param>
    /// <param name="stockQuantity">The initial stock quantity. Must be non-negative.</param>
    /// <exception cref="ArgumentException">Thrown when name is null/whitespace, currency is invalid, or stockQuantity is negative.</exception>
    /// <exception cref="ArgumentNullException">Thrown when sku is null.</exception>
    public ProductVariant(
        Guid productId,
        Sku sku,
        string name,
        decimal priceAdjustmentAmount,
        string priceAdjustmentCurrency,
        int stockQuantity = 0)
    {
        if (productId == Guid.Empty)
        {
            throw new ArgumentException("Product ID cannot be empty.", nameof(productId));
        }

        ProductId = productId;
        SetSku(sku);
        SetName(name);
        SetPriceAdjustment(priceAdjustmentAmount, priceAdjustmentCurrency);
        SetStockQuantity(stockQuantity);
    }

    /// <summary>
    /// Calculates the final price for this variant based on the base product price.
    /// </summary>
    /// <param name="basePrice">The base price of the product.</param>
    /// <returns>The final price for this variant.</returns>
    /// <exception cref="InvalidOperationException">Thrown when currencies don't match.</exception>
    /// <remarks>
    /// This method is maintained for backward compatibility.
    /// Consider using <see cref="GetFinalPriceWithInfo"/> for more detailed information.
    /// </remarks>
    public Money GetFinalPrice(Money basePrice)
    {
        return GetFinalPriceWithInfo(basePrice).Price;
    }

    /// <summary>
    /// Calculates the final price for this variant based on the base product price,
    /// returning additional information about the calculation.
    /// </summary>
    /// <param name="basePrice">The base price of the product.</param>
    /// <returns>A <see cref="FinalPriceResult"/> containing the final price and whether it was clamped.</returns>
    /// <exception cref="InvalidOperationException">Thrown when currencies don't match.</exception>
    /// <remarks>
    /// If the calculated price would be negative, it is clamped to zero.
    /// The <see cref="FinalPriceResult.WasClampedToZero"/> property indicates when this occurs,
    /// allowing calling code to log a warning or take other appropriate action.
    /// </remarks>
    public FinalPriceResult GetFinalPriceWithInfo(Money basePrice)
    {
        ArgumentNullException.ThrowIfNull(basePrice);

        if (basePrice.Currency != PriceAdjustmentCurrency)
        {
            throw new InvalidOperationException(
                $"Cannot calculate final price: base price currency ({basePrice.Currency}) " +
                $"does not match price adjustment currency ({PriceAdjustmentCurrency}).");
        }

        var finalAmount = basePrice.Amount + PriceAdjustmentAmount;
        var wasClampedToZero = false;

        // Ensure the final price is not negative
        if (finalAmount < 0)
        {
            wasClampedToZero = true;
            finalAmount = 0;
        }

        return new FinalPriceResult(new Money(finalAmount, basePrice.Currency), wasClampedToZero);
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
    /// <param name="amount">The new price adjustment amount (can be negative for discounts).</param>
    /// <param name="currency">The currency code for the price adjustment.</param>
    /// <exception cref="ArgumentException">Thrown when currency is invalid.</exception>
    public void UpdatePriceAdjustment(decimal amount, string currency)
    {
        SetPriceAdjustment(amount, currency);
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

    private void SetPriceAdjustment(decimal amount, string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new ArgumentException("Currency is required.", nameof(currency));
        }

        if (currency.Length != 3)
        {
            throw new ArgumentException("Currency must be a 3-character code.", nameof(currency));
        }

        PriceAdjustmentAmount = amount;
        PriceAdjustmentCurrency = currency.ToUpperInvariant();
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
