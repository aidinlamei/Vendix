namespace Vendix.Application.Common.Interfaces;

/// <summary>
/// Provides access to the current date and time.
/// </summary>
/// <remarks>
/// This abstraction allows for easier testing by enabling the injection
/// of a mock or fixed time provider during unit tests.
/// </remarks>
public interface IDateTimeProvider
{
    /// <summary>
    /// Gets the current UTC date and time.
    /// </summary>
    DateTime UtcNow { get; }
}
