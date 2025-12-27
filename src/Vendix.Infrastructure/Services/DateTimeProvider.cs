using Vendix.Application.Common.Interfaces;

namespace Vendix.Infrastructure.Services;

/// <summary>
/// Default implementation of <see cref="IDateTimeProvider"/> that returns the current UTC time.
/// </summary>
/// <remarks>
/// This implementation uses DateTime.UtcNow. For testing purposes, consider using
/// a mock implementation that allows controlling the returned time.
/// </remarks>
public sealed class DateTimeProvider : IDateTimeProvider
{
    /// <inheritdoc />
    public DateTime UtcNow => DateTime.UtcNow;
}
