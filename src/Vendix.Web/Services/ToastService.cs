namespace Vendix.Web.Services;

/// <summary>
/// Represents a single toast notification.
/// </summary>
public record ToastMessage(string Type, string Message, string? Title = null);

/// <summary>
/// Global toast notification service. Components push messages and the
/// <c>ToastHost</c> component renders them.
/// </summary>
public class ToastService
{
    private readonly List<ToastMessage> _toasts = [];

    /// <summary>
    /// Occurs when the toast collection changes.
    /// </summary>
    public event Action? ToastsChanged;

    /// <summary>
    /// Gets the active toast messages.
    /// </summary>
    public IReadOnlyList<ToastMessage> Toasts => _toasts;

    public void ShowSuccess(string message, string? title = null) => Show("success", message, title);

    public void ShowError(string message, string? title = null) => Show("error", message, title);

    public void ShowWarning(string message, string? title = null) => Show("warning", message, title);

    public void ShowInfo(string message, string? title = null) => Show("info", message, title);

    public void Show(string type, string message, string? title = null)
    {
        _toasts.Add(new ToastMessage(type, message, title));
        ToastsChanged?.Invoke();
    }

    public void Remove(ToastMessage toast)
    {
        _toasts.Remove(toast);
        ToastsChanged?.Invoke();
    }
}
