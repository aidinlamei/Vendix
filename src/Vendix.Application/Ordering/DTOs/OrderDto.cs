namespace Vendix.Application.Ordering.DTOs;

/// <summary>
/// DTO for a single order line item.
/// </summary>
public sealed class OrderItemDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public string? ImageUrl { get; set; }
    public decimal LineTotal { get; set; }
}

/// <summary>
/// DTO for a full order (detail view).
/// </summary>
public sealed class OrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string BuyerEmail { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrderItemDto> Items { get; set; } = [];
}

/// <summary>
/// Lightweight DTO for order lists (admin index, "my orders").
/// </summary>
public sealed class OrderListDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string BuyerEmail { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int ItemCount { get; set; }
}

/// <summary>
/// Result returned immediately after successfully placing an order.
/// </summary>
public sealed record PlaceOrderResultDto(Guid OrderId, string OrderNumber, decimal Total, string Currency);
