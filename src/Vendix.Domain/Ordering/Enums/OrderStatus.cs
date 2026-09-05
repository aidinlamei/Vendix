namespace Vendix.Domain.Ordering.Enums;

/// <summary>
/// Represents the lifecycle status of an order.
/// </summary>
public enum OrderStatus
{
    /// <summary>The order was placed and is awaiting processing.</summary>
    Pending = 0,

    /// <summary>The order is being prepared/packed.</summary>
    Processing = 1,

    /// <summary>The order has been handed to a carrier.</summary>
    Shipped = 2,

    /// <summary>The order has been delivered to the buyer.</summary>
    Delivered = 3,

    /// <summary>The order was cancelled and will not be fulfilled.</summary>
    Cancelled = 4
}
