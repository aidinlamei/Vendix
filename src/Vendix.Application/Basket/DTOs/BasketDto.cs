namespace Vendix.Application.Basket.DTOs;

/// <summary>
/// DTO for a single basket line item.
/// </summary>
public sealed class BasketItemDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSlug { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public string Currency { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? ImageUrl { get; set; }
    public decimal LineTotal { get; set; }
}

/// <summary>
/// DTO for a buyer's basket.
/// </summary>
public sealed class BasketDto
{
    public Guid Id { get; set; }
    public string BuyerId { get; set; } = string.Empty;
    public List<BasketItemDto> Items { get; set; } = [];
    public decimal Subtotal { get; set; }
    public string? Currency { get; set; }
    public int ItemCount { get; set; }
}
