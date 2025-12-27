using Vendix.Domain.Common;

namespace Vendix.Domain.Catalog.ValueObjects;

/// <summary>
/// Represents a monetary value with an amount and currency.
/// </summary>
/// <remarks>
/// Money is a value object that encapsulates a decimal amount and a currency code.
/// It is immutable and provides validation to ensure monetary values are valid.
/// </remarks>
public sealed class Money : ValueObject
{
    /// <summary>
    /// Gets the monetary amount.
    /// </summary>
    public decimal Amount { get; }

    /// <summary>
    /// Gets the currency code (e.g., "USD", "EUR", "IRR").
    /// </summary>
    public string Currency { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Money"/> class.
    /// </summary>
    /// <param name="amount">The monetary amount. Must be non-negative.</param>
    /// <param name="currency">The currency code. Must be a non-empty 3-character code.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when amount is negative or currency is invalid.
    /// </exception>
    public Money(decimal amount, string currency)
    {
        if (amount < 0)
        {
            throw new ArgumentException("Amount cannot be negative.", nameof(amount));
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new ArgumentException("Currency is required.", nameof(currency));
        }

        if (currency.Length != 3)
        {
            throw new ArgumentException("Currency must be a 3-character code.", nameof(currency));
        }

        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }

    /// <summary>
    /// Creates a zero Money value with the specified currency.
    /// </summary>
    /// <param name="currency">The currency code.</param>
    /// <returns>A new Money instance with zero amount.</returns>
    public static Money Zero(string currency) => new(0, currency);

    /// <summary>
    /// Adds two Money values together.
    /// </summary>
    /// <param name="other">The Money value to add.</param>
    /// <returns>A new Money value representing the sum.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when currencies do not match.
    /// </exception>
    public Money Add(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);

        if (Currency != other.Currency)
        {
            throw new InvalidOperationException($"Cannot add money with different currencies: {Currency} and {other.Currency}.");
        }

        return new Money(Amount + other.Amount, Currency);
    }

    /// <summary>
    /// Subtracts a Money value from this value.
    /// </summary>
    /// <param name="other">The Money value to subtract.</param>
    /// <returns>A new Money value representing the difference.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when currencies do not match or result would be negative.
    /// </exception>
    public Money Subtract(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);

        if (Currency != other.Currency)
        {
            throw new InvalidOperationException($"Cannot subtract money with different currencies: {Currency} and {other.Currency}.");
        }

        var result = Amount - other.Amount;
        if (result < 0)
        {
            throw new InvalidOperationException("Subtraction would result in a negative amount.");
        }

        return new Money(result, Currency);
    }

    /// <summary>
    /// Multiplies this Money value by a factor.
    /// </summary>
    /// <param name="factor">The multiplication factor. Must be non-negative.</param>
    /// <returns>A new Money value representing the product.</returns>
    /// <exception cref="ArgumentException">Thrown when factor is negative.</exception>
    public Money Multiply(decimal factor)
    {
        if (factor < 0)
        {
            throw new ArgumentException("Factor cannot be negative.", nameof(factor));
        }

        return new Money(Amount * factor, Currency);
    }

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    /// <summary>
    /// Returns a string representation of this Money value.
    /// </summary>
    /// <returns>A string in the format "Amount Currency".</returns>
    public override string ToString() => $"{Amount:F2} {Currency}";

    /// <summary>
    /// Adds two Money values together.
    /// </summary>
    public static Money operator +(Money left, Money right) => left.Add(right);

    /// <summary>
    /// Subtracts one Money value from another.
    /// </summary>
    public static Money operator -(Money left, Money right) => left.Subtract(right);

    /// <summary>
    /// Multiplies a Money value by a factor.
    /// </summary>
    public static Money operator *(Money money, decimal factor) => money.Multiply(factor);

    /// <summary>
    /// Multiplies a Money value by a factor.
    /// </summary>
    public static Money operator *(decimal factor, Money money) => money.Multiply(factor);
}
