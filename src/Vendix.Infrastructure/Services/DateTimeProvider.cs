using Vendix.Application.Common.Interfaces;

namespace Vendix.Infrastructure.Services;

/// <summary>
/// Provides the current date and time using the system clock.
/// </summary>
/// <remarks>
/// This implementation uses <see cref="DateTime.UtcNow"/> for production scenarios.
/// For testing, inject a mock implementation that returns a fixed time.
/// </remarks>
public sealed class DateTimeProvider : IDateTimeProvider
{
    /// <inheritdoc />
    public DateTime UtcNow => DateTime.UtcNow;
}
