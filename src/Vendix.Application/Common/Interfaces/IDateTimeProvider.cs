namespace Vendix.Application.Common.Interfaces;

/// <summary>
/// Provides an abstraction for accessing the current date and time.
/// </summary>
/// <remarks>
/// Using this interface instead of DateTime.UtcNow directly enables:
/// - Testability by allowing time to be mocked in unit tests
/// - Consistency across the application for time-related operations
/// - Future flexibility for time zone handling or other time-related requirements
/// </remarks>
public interface IDateTimeProvider
{
    /// <summary>
    /// Gets the current UTC date and time.
    /// </summary>
    /// <value>The current date and time expressed as UTC.</value>
    DateTime UtcNow { get; }
}
