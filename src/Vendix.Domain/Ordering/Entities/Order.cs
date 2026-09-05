using Vendix.Domain.Common;
using Vendix.Domain.Ordering.Enums;
using Vendix.Domain.Ordering.ValueObjects;

namespace Vendix.Domain.Ordering.Entities;

/// <summary>
/// Represents a placed customer order.
/// </summary>
/// <remarks>
/// Order is an aggregate root. It is created from a <see cref="Basket.Entities.Basket"/> at
/// checkout time (see the Application layer's PlaceOrderCommand) and is immutable except for
/// its <see cref="Status"/>.
/// </remarks>
public class Order : AggregateRoot, IAuditableEntity
{
    private readonly List<OrderItem> _items = [];

    /// <summary>Gets the human-readable order number.</summary>
    public OrderNumber OrderNumber { get; private set; } = null!;

    /// <summary>Gets the identifier of the buyer who placed this order.</summary>
    public string BuyerId { get; private set; } = null!;

    /// <summary>Gets the buyer's contact email for order updates.</summary>
    public string BuyerEmail { get; private set; } = null!;

    /// <summary>
    /// Gets the shipping address as free text. A structured <c>Address</c> value object
    /// (street/city/postal code/country) is deferred to Phase 7 (Inventory &amp; Shipping)
    /// per <c>docs/ARCHITECTURE.md</c> §4 — this is an intentional, documented simplification.
    /// </summary>
    public string ShippingAddress { get; private set; } = null!;

    /// <summary>Gets the current order status.</summary>
    public OrderStatus Status { get; private set; }

    /// <summary>Gets the currency code for all monetary amounts on this order.</summary>
    public string Currency { get; private set; } = null!;

    /// <summary>Gets the shipping cost.</summary>
    public decimal ShippingCost { get; private set; }

    /// <summary>Gets the order's line items.</summary>
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    /// <summary>Gets the sum of all line totals, excluding shipping.</summary>
    public decimal Subtotal => _items.Sum(i => i.LineTotal);

    /// <summary>Gets the grand total including shipping.</summary>
    public decimal Total => Subtotal + ShippingCost;

    /// <inheritdoc />
    public DateTime CreatedAt { get; set; }

    /// <inheritdoc />
    public string? CreatedBy { get; set; }

    /// <inheritdoc />
    public DateTime? ModifiedAt { get; set; }

    /// <inheritdoc />
    public string? ModifiedBy { get; set; }

    /// <summary>
    /// Required by EF Core for materialization.
    /// </summary>
    private Order() { }

    /// <summary>
    /// Initializes a new order in <see cref="OrderStatus.Pending"/> status with a freshly
    /// generated order number.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when buyerId, buyerEmail, shippingAddress, or currency is null/whitespace,
    /// or when shippingCost is negative.
    /// </exception>
    public Order(string buyerId, string buyerEmail, string shippingAddress, string currency, decimal shippingCost)
    {
        if (string.IsNullOrWhiteSpace(buyerId))
        {
            throw new ArgumentException("Buyer id is required.", nameof(buyerId));
        }

        if (string.IsNullOrWhiteSpace(buyerEmail))
        {
            throw new ArgumentException("Buyer email is required.", nameof(buyerEmail));
        }

        if (string.IsNullOrWhiteSpace(shippingAddress))
        {
            throw new ArgumentException("Shipping address is required.", nameof(shippingAddress));
        }

        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
        {
            throw new ArgumentException("Currency must be a 3-character code.", nameof(currency));
        }

        if (shippingCost < 0)
        {
            throw new ArgumentException("Shipping cost cannot be negative.", nameof(shippingCost));
        }

        OrderNumber = OrderNumber.Generate();
        BuyerId = buyerId;
        BuyerEmail = buyerEmail;
        ShippingAddress = shippingAddress;
        Currency = currency.ToUpperInvariant();
        ShippingCost = shippingCost;
        Status = OrderStatus.Pending;
    }

    /// <summary>
    /// Adds a line item to the order. Should only be called while building the order at
    /// checkout time, before it is persisted.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when quantity is not positive or unitPrice is negative.</exception>
    public void AddItem(Guid productId, string productName, string sku, decimal unitPrice, int quantity, string? imageUrl)
    {
        _items.Add(new OrderItem(Id, productId, productName, sku, unitPrice, quantity, imageUrl));
    }

    /// <summary>
    /// Cancels the order.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the order is already cancelled, or has already shipped/been delivered.
    /// </exception>
    public void Cancel()
    {
        if (Status == OrderStatus.Cancelled)
        {
            throw new InvalidOperationException("Order is already cancelled.");
        }

        if (Status is OrderStatus.Shipped or OrderStatus.Delivered)
        {
            throw new InvalidOperationException($"Cannot cancel an order with status {Status}.");
        }

        Status = OrderStatus.Cancelled;
    }

    /// <summary>
    /// Updates the order status (admin action).
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the order is already cancelled.</exception>
    public void UpdateStatus(OrderStatus status)
    {
        if (Status == OrderStatus.Cancelled)
        {
            throw new InvalidOperationException("Cannot change the status of a cancelled order.");
        }

        if (status == OrderStatus.Cancelled)
        {
            Cancel();
            return;
        }

        Status = status;
    }
}
