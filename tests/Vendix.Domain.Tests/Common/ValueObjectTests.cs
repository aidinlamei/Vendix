using Vendix.Domain.Common;

namespace Vendix.Domain.Tests.Common;

/// <summary>
/// Unit tests for <see cref="ValueObject"/> class.
/// </summary>
public class ValueObjectTests
{
    /// <summary>
    /// Simple value object for testing with primitive types.
    /// </summary>
    private sealed class Money : ValueObject
    {
        public decimal Amount { get; }
        public string Currency { get; }

        public Money(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }
    }

    /// <summary>
    /// Value object with nullable property for testing null handling.
    /// </summary>
    private sealed class Address : ValueObject
    {
        public string Street { get; }
        public string City { get; }
        public string? PostalCode { get; }

        public Address(string street, string city, string? postalCode = null)
        {
            Street = street;
            City = city;
            PostalCode = postalCode;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Street;
            yield return City;
            yield return PostalCode;
        }
    }

    /// <summary>
    /// Different value object type with same structure for testing type safety.
    /// </summary>
    private sealed class Price : ValueObject
    {
        public decimal Amount { get; }
        public string Currency { get; }

        public Price(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }
    }

    [Fact]
    public void Equals_SameValues_ReturnsTrue()
    {
        // Arrange
        var money1 = new Money(100.00m, "USD");
        var money2 = new Money(100.00m, "USD");

        // Act & Assert
        money1.Equals(money2).Should().BeTrue();
        (money1 == money2).Should().BeTrue();
        (money1 != money2).Should().BeFalse();
    }

    [Fact]
    public void Equals_DifferentAmount_ReturnsFalse()
    {
        // Arrange
        var money1 = new Money(100.00m, "USD");
        var money2 = new Money(200.00m, "USD");

        // Act & Assert
        money1.Equals(money2).Should().BeFalse();
        (money1 == money2).Should().BeFalse();
        (money1 != money2).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentCurrency_ReturnsFalse()
    {
        // Arrange
        var money1 = new Money(100.00m, "USD");
        var money2 = new Money(100.00m, "EUR");

        // Act & Assert
        money1.Equals(money2).Should().BeFalse();
    }

    [Fact]
    public void Equals_SameReference_ReturnsTrue()
    {
        // Arrange
        var money = new Money(100.00m, "USD");

        // Act & Assert
        money.Equals(money).Should().BeTrue();
#pragma warning disable CS1718 // Comparison made to same variable
        (money == money).Should().BeTrue();
#pragma warning restore CS1718
    }

    [Fact]
    public void Equals_Null_ReturnsFalse()
    {
        // Arrange
        var money = new Money(100.00m, "USD");

        // Act & Assert
        money.Equals(null).Should().BeFalse();
        (money == null).Should().BeFalse();
        (null == money).Should().BeFalse();
    }

    [Fact]
    public void Equals_BothNull_ReturnsTrue()
    {
        // Arrange
        Money? money1 = null;
        Money? money2 = null;

        // Act & Assert
        (money1 == money2).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentType_ReturnsFalse()
    {
        // Arrange
        var money = new Money(100.00m, "USD");
        var price = new Price(100.00m, "USD");

        // Act & Assert
        money.Equals(price).Should().BeFalse();
    }

    [Fact]
    public void Equals_NonValueObject_ReturnsFalse()
    {
        // Arrange
        var money = new Money(100.00m, "USD");
        var notAValueObject = "not a value object";

        // Act & Assert
        money.Equals(notAValueObject).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithNullableProperty_BothNull_ReturnsTrue()
    {
        // Arrange
        var address1 = new Address("123 Main St", "New York");
        var address2 = new Address("123 Main St", "New York");

        // Act & Assert
        address1.Equals(address2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithNullableProperty_BothHaveValue_ReturnsTrue()
    {
        // Arrange
        var address1 = new Address("123 Main St", "New York", "10001");
        var address2 = new Address("123 Main St", "New York", "10001");

        // Act & Assert
        address1.Equals(address2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithNullableProperty_OneNull_ReturnsFalse()
    {
        // Arrange
        var address1 = new Address("123 Main St", "New York", "10001");
        var address2 = new Address("123 Main St", "New York");

        // Act & Assert
        address1.Equals(address2).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_SameValues_ReturnsSameHashCode()
    {
        // Arrange
        var money1 = new Money(100.00m, "USD");
        var money2 = new Money(100.00m, "USD");

        // Act & Assert
        money1.GetHashCode().Should().Be(money2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentValues_ReturnsDifferentHashCode()
    {
        // Arrange
        var money1 = new Money(100.00m, "USD");
        var money2 = new Money(200.00m, "USD");

        // Act & Assert
        money1.GetHashCode().Should().NotBe(money2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_WithNullableProperty_HandlesNullCorrectly()
    {
        // Arrange
        var address1 = new Address("123 Main St", "New York");
        var address2 = new Address("123 Main St", "New York");

        // Act & Assert
        address1.GetHashCode().Should().Be(address2.GetHashCode());
    }

    [Fact]
    public void ValueObject_IsImmutable_PropertyValuesPreserved()
    {
        // Arrange & Act
        var money = new Money(100.00m, "USD");

        // Assert
        money.Amount.Should().Be(100.00m);
        money.Currency.Should().Be("USD");
    }
}
