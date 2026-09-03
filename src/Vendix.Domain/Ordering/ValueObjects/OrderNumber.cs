using System.Text.RegularExpressions;
using Vendix.Domain.Common;

namespace Vendix.Domain.Ordering.ValueObjects;

/// <summary>
/// Represents a human-readable, unique order number in the format "ORD-yyyyMMdd-XXXXXX".
/// </summary>
/// <remarks>
/// The random 6-character suffix (uppercase hex, from a GUID fragment) makes collisions
/// astronomically unlikely for a small/medium store; if this store grows to a volume where
/// that risk becomes real, replace <see cref="Generate"/> with a database sequence instead
/// of changing this value object's shape.
/// </remarks>
public sealed partial class OrderNumber : ValueObject
{
    /// <summary>
    /// The fixed length of a valid order number: "ORD-" (4) + 8 date digits + "-" (1) + 6 suffix chars = 19.
    /// </summary>
    public const int Length = 19;

    /// <summary>
    /// The regex pattern for validating order number format.
    /// </summary>
    public const string Pattern = @"^ORD-\d{8}-[A-F0-9]{6}$";

    /// <summary>
    /// Gets the order number value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderNumber"/> class from an existing value
    /// (used when reading from the database).
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when value is null, empty, or invalid format.</exception>
    public OrderNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !OrderNumberPattern().IsMatch(value))
        {
            throw new ArgumentException("Invalid order number format.", nameof(value));
        }

        Value = value;
    }

    /// <summary>
    /// Generates a new, unique order number based on the current UTC date.
    /// </summary>
    public static OrderNumber Generate()
    {
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
        var suffix = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
        return new OrderNumber($"ORD-{datePart}-{suffix}");
    }

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <summary>
    /// Returns the order number value as a string.
    /// </summary>
    public override string ToString() => Value;

    /// <summary>
    /// Implicitly converts an OrderNumber to its string value.
    /// </summary>
    public static implicit operator string(OrderNumber orderNumber) => orderNumber.Value;

    [GeneratedRegex(@"^ORD-\d{8}-[A-F0-9]{6}$", RegexOptions.Compiled)]
    private static partial Regex OrderNumberPattern();
}
