using MediatR;
using Vendix.Application.Basket.Commands;
using Vendix.Application.Basket.DTOs;
using Vendix.Application.Basket.Queries;

namespace Vendix.Web.Services;

/// <summary>
/// Client-facing cart service backed by the server-side Basket aggregate (see
/// <c>Vendix.Application.Basket</c>). Keeps the same public surface the UI already depends on
/// (Items, ItemCount, Subtotal, Currency, Changed) while persisting through MediatR
/// commands/queries instead of localStorage, so a basket now survives across devices/tabs for
/// the same buyer ID (see <see cref="BuyerIdProvider"/>).
/// </summary>
public class CartService
{
    private readonly IMediator _mediator;
    private readonly BuyerIdProvider _buyerIdProvider;
    private readonly ILogger<CartService> _logger;
    private List<CartItem> _items = [];
    private bool _initialized;

    /// <summary>
    /// Occurs whenever the cart contents change (add, update, remove, clear, load).
    /// </summary>
    public event Action? Changed;

    /// <summary>
    /// Initializes a new instance of the <see cref="CartService"/> class.
    /// </summary>
    public CartService(IMediator mediator, BuyerIdProvider buyerIdProvider, ILogger<CartService> logger)
    {
        _mediator = mediator;
        _buyerIdProvider = buyerIdProvider;
        _logger = logger;
    }

    /// <summary>
    /// Gets the current cart items.
    /// </summary>
    public IReadOnlyList<CartItem> Items => _items;

    /// <summary>
    /// Gets the total number of items (sum of quantities).
    /// </summary>
    public int ItemCount => _items.Sum(i => i.Quantity);

    /// <summary>
    /// Gets the cart subtotal (sum of line totals).
    /// </summary>
    public decimal Subtotal => _items.Sum(i => i.LineTotal);

    /// <summary>
    /// Gets the currency of the first item, if any.
    /// </summary>
    public string? Currency => _items.FirstOrDefault()?.Currency;

    /// <summary>
    /// Loads the basket from the server. Safe to call multiple times.
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;

        try
        {
            var buyerId = await _buyerIdProvider.GetOrCreateAsync();
            var basket = await _mediator.Send(new GetBasketQuery(buyerId));
            _items = MapToCartItems(basket);
            Changed?.Invoke();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not load basket.");
        }
    }

    /// <summary>
    /// Adds a product to the basket, merging quantities if it's already present. Only
    /// <see cref="CartItem.ProductId"/> and <see cref="CartItem.Quantity"/> are sent to the
    /// server — price/name/etc. are re-resolved server-side from the authoritative product.
    /// </summary>
    public async Task AddAsync(CartItem item)
    {
        var buyerId = await _buyerIdProvider.GetOrCreateAsync();
        var result = await _mediator.Send(new AddToBasketCommand(buyerId, item.ProductId, item.Quantity));

        if (result.IsSuccess)
        {
            _items = MapToCartItems(result.Value);
            Changed?.Invoke();
        }
        else
        {
            _logger.LogWarning("Could not add product {ProductId} to basket: {Error}", item.ProductId, result.Error);
        }
    }

    /// <summary>
    /// Sets the quantity of an item. A quantity of zero or less removes the item.
    /// </summary>
    public async Task SetQuantityAsync(Guid productId, int quantity)
    {
        var buyerId = await _buyerIdProvider.GetOrCreateAsync();
        var result = await _mediator.Send(new UpdateBasketItemQuantityCommand(buyerId, productId, quantity));

        if (result.IsSuccess)
        {
            _items = MapToCartItems(result.Value);
            Changed?.Invoke();
        }
    }

    /// <summary>
    /// Removes an item from the cart.
    /// </summary>
    public async Task RemoveAsync(Guid productId)
    {
        var buyerId = await _buyerIdProvider.GetOrCreateAsync();
        var result = await _mediator.Send(new RemoveFromBasketCommand(buyerId, productId));

        if (result.IsSuccess)
        {
            _items = MapToCartItems(result.Value);
            Changed?.Invoke();
        }
    }

    /// <summary>
    /// Removes all items from the cart.
    /// </summary>
    public async Task ClearAsync()
    {
        var buyerId = await _buyerIdProvider.GetOrCreateAsync();
        await _mediator.Send(new ClearBasketCommand(buyerId));
        _items = [];
        Changed?.Invoke();
    }

    /// <summary>
    /// Gets the current quantity of a product in the cart.
    /// </summary>
    public int GetQuantity(Guid productId)
        => _items.FirstOrDefault(i => i.ProductId == productId)?.Quantity ?? 0;

    /// <summary>
    /// Gets the current buyer ID. Used by the Checkout page to place an order against the
    /// same basket this service manages.
    /// </summary>
    public Task<string> GetBuyerIdAsync() => _buyerIdProvider.GetOrCreateAsync();

    private static List<CartItem> MapToCartItems(BasketDto basket) =>
        basket.Items.Select(i => new CartItem
        {
            ProductId = i.ProductId,
            Slug = i.ProductSlug,
            Name = i.ProductName,
            Sku = i.Sku,
            Price = i.UnitPrice,
            Currency = i.Currency,
            ImageUrl = i.ImageUrl,
            Quantity = i.Quantity
        }).ToList();
}
