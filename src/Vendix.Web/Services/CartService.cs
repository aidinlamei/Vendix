using System.Text.Json;
using Microsoft.JSInterop;

namespace Vendix.Web.Services;

/// <summary>
/// Manages the guest shopping cart. Items are persisted in localStorage via JS interop
/// and kept in memory so multiple components stay in sync through the <see cref="Changed"/> event.
/// </summary>
public class CartService
{
    private const string StorageKey = "vendix.cart";
    private const string JsNamespace = "vendix.cart";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IJSRuntime _js;
    private readonly ILogger<CartService> _logger;
    private List<CartItem> _items = [];
    private bool _initialized;

    /// <summary>
    /// Occurs whenever the cart contents change (add, update, remove, clear, load).
    /// </summary>
    public event Action? Changed;

    public CartService(IJSRuntime js, ILogger<CartService> logger)
    {
        _js = js;
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
    /// Loads the cart from localStorage. Safe to call multiple times.
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_initialized)
            return;

        _initialized = true;

        try
        {
            var json = await _js.InvokeAsync<string>($"{JsNamespace}.load");
            if (string.IsNullOrWhiteSpace(json))
                return;

            var loaded = JsonSerializer.Deserialize<List<CartItem>>(json, JsonOptions);
            if (loaded is not null && loaded.Count > 0)
            {
                _items = loaded;
                Changed?.Invoke();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not load cart from localStorage.");
        }
    }

    /// <summary>
    /// Adds an item to the cart, merging quantities if the product already exists.
    /// </summary>
    public async Task AddAsync(CartItem item)
    {
        var existing = _items.FirstOrDefault(i => i.ProductId == item.ProductId);
        if (existing is not null)
        {
            existing.Quantity += item.Quantity;
        }
        else
        {
            _items.Add(item);
        }

        await PersistAsync();
        Changed?.Invoke();
    }

    /// <summary>
    /// Sets the quantity of an item. A quantity of zero or less removes the item.
    /// </summary>
    public async Task SetQuantityAsync(Guid productId, int quantity)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item is null)
            return;

        if (quantity <= 0)
        {
            _items.Remove(item);
        }
        else
        {
            item.Quantity = quantity;
        }

        await PersistAsync();
        Changed?.Invoke();
    }

    /// <summary>
    /// Removes an item from the cart.
    /// </summary>
    public async Task RemoveAsync(Guid productId)
    {
        if (_items.RemoveAll(i => i.ProductId == productId) == 0)
            return;

        await PersistAsync();
        Changed?.Invoke();
    }

    /// <summary>
    /// Removes all items from the cart.
    /// </summary>
    public async Task ClearAsync()
    {
        if (_items.Count == 0)
            return;

        _items.Clear();
        await PersistAsync();
        Changed?.Invoke();
    }

    /// <summary>
    /// Gets the current quantity of a product in the cart.
    /// </summary>
    public int GetQuantity(Guid productId)
        => _items.FirstOrDefault(i => i.ProductId == productId)?.Quantity ?? 0;

    private async Task PersistAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_items, JsonOptions);
            if (_items.Count == 0)
            {
                await _js.InvokeVoidAsync($"{JsNamespace}.clear");
            }
            else
            {
                await _js.InvokeVoidAsync($"{JsNamespace}.save", json);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not persist cart to localStorage.");
        }
    }
}
