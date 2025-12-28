using System.Text.RegularExpressions;
using Vendix.Domain.Common;

namespace Vendix.Domain.Catalog.ValueObjects;

/// <summary>
/// Represents a Stock Keeping Unit (SKU) for a product.
/// </summary>
/// <remarks>
/// SKU is a unique identifier for a product variant used for inventory tracking.
/// It must be alphanumeric (letters, numbers, and hyphens) and between 3 and 50 characters.
/// </remarks>
public sealed partial class Sku : ValueObject
{
    /// <summary>
    /// The minimum length allowed for a SKU.
    /// </summary>
    public const int MinLength = 3;

    /// <summary>
    /// The maximum length allowed for a SKU.
    /// </summary>
    public const int MaxLength = 50;

    /// <summary>
    /// The regex pattern for validating SKU format: alphanumeric and hyphens only.
    /// </summary>
    public const string Pattern = @"^[A-Za-z0-9\-]+$";

    /// <summary>
    /// Gets the SKU value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Sku"/> class.
    /// </summary>
    /// <param name="value">The SKU value. Must be alphanumeric and between 3-50 characters.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the value is null, empty, invalid format, or outside the length bounds.
    /// </exception>
    public Sku(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("SKU is required.", nameof(value));
        }

        if (value.Length < MinLength)
        {
            throw new ArgumentException($"SKU must be at least {MinLength} characters.", nameof(value));
        }

        if (value.Length > MaxLength)
        {
            throw new ArgumentException($"SKU cannot exceed {MaxLength} characters.", nameof(value));
        }

        if (!SkuPattern().IsMatch(value))
        {
            throw new ArgumentException("SKU must contain only letters, numbers, and hyphens.", nameof(value));
        }

        Value = value.ToUpperInvariant();
    }

    /// <summary>
    /// Validates whether the given string is a valid SKU format.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <returns>True if the value is a valid SKU format; otherwise, false.</returns>
    public static bool IsValid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (value.Length < MinLength || value.Length > MaxLength)
        {
            return false;
        }

        return SkuPattern().IsMatch(value);
    }

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <summary>
    /// Returns the SKU value as a string.
    /// </summary>
    /// <returns>The SKU value.</returns>
    public override string ToString() => Value;

    /// <summary>
    /// Implicitly converts a Sku to its string value.
    /// </summary>
    /// <param name="sku">The SKU to convert.</param>
    public static implicit operator string(Sku sku) => sku.Value;

    /// <summary>
    /// Regex pattern for validating SKU format: alphanumeric and hyphens only.
    /// </summary>
    [GeneratedRegex(@"^[A-Za-z0-9\-]+$", RegexOptions.Compiled)]
    private static partial Regex SkuPattern();
}
