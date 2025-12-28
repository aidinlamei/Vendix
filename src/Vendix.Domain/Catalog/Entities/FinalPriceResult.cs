using Vendix.Domain.Catalog.ValueObjects;

namespace Vendix.Domain.Catalog.Entities;

/// <summary>
/// Represents the result of calculating a variant's final price.
/// </summary>
/// <remarks>
/// This type provides information about whether the price was clamped to zero,
/// allowing calling code to log warnings or take other actions.
/// </remarks>
/// <param name="Price">The calculated final price.</param>
/// <param name="WasClampedToZero">True if the calculated price was negative and clamped to zero.</param>
public sealed record FinalPriceResult(Money Price, bool WasClampedToZero);
