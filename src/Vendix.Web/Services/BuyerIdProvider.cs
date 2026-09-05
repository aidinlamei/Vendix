using Microsoft.JSInterop;

namespace Vendix.Web.Services;

/// <summary>
/// Resolves a stable anonymous buyer identifier for the current browser, persisted in
/// localStorage. Used to associate a guest's basket and orders across visits until a full
/// authentication system (Phase 5, currently 0% per docs/ARCHITECTURE.md) replaces it with
/// the logged-in user's ID.
/// </summary>
public class BuyerIdProvider
{
    private const string JsNamespace = "vendix.buyer";

    private readonly IJSRuntime _js;
    private string? _buyerId;

    /// <summary>
    /// Initializes a new instance of the <see cref="BuyerIdProvider"/> class.
    /// </summary>
    public BuyerIdProvider(IJSRuntime js)
    {
        _js = js;
    }

    /// <summary>
    /// Gets the current buyer ID, creating and persisting a new one on first call.
    /// Cached in-memory for the lifetime of this scoped service (one Blazor circuit).
    /// </summary>
    public async Task<string> GetOrCreateAsync()
    {
        if (_buyerId is not null)
        {
            return _buyerId;
        }

        var existing = await _js.InvokeAsync<string>($"{JsNamespace}.load");
        if (!string.IsNullOrWhiteSpace(existing))
        {
            _buyerId = existing;
            return _buyerId;
        }

        _buyerId = Guid.NewGuid().ToString("N");
        await _js.InvokeVoidAsync($"{JsNamespace}.save", _buyerId);
        return _buyerId;
    }
}
