using Vendix.Domain.Catalog.ValueObjects;

namespace Vendix.Domain.Tests.Catalog;

/// <summary>
/// Unit tests for <see cref="Money"/> value object.
/// </summary>
public class MoneyTests
{
    [Fact]
    public void Constructor_ValidInputs_CreatesMoney()
    {
        // Arrange & Act
        var money = new Money(100.00m, "USD");

        // Assert
        money.Amount.Should().Be(100.00m);
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Constructor_LowercaseCurrency_NormalizesToUppercase()
    {
        // Arrange & Act
        var money = new Money(50.00m, "eur");

        // Assert
        money.Currency.Should().Be("EUR");
    }

    [Fact]
    public void Constructor_NegativeAmount_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => new Money(-10.00m, "USD");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Amount cannot be negative.*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Constructor_EmptyOrNullCurrency_ThrowsArgumentException(string? currency)
    {
        // Arrange & Act
        var act = () => new Money(100.00m, currency!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Currency is required.*");
    }

    [Theory]
    [InlineData("US")]
    [InlineData("USDD")]
    public void Constructor_InvalidCurrencyLength_ThrowsArgumentException(string currency)
    {
        // Arrange & Act
        var act = () => new Money(100.00m, currency);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Currency must be a 3-character code.*");
    }

    [Fact]
    public void Zero_CreatesMoney_WithZeroAmount()
    {
        // Arrange & Act
        var money = Money.Zero("USD");

        // Assert
        money.Amount.Should().Be(0);
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Add_SameCurrency_ReturnsSum()
    {
        // Arrange
        var money1 = new Money(100.00m, "USD");
        var money2 = new Money(50.00m, "USD");

        // Act
        var result = money1.Add(money2);

        // Assert
        result.Amount.Should().Be(150.00m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Add_DifferentCurrency_ThrowsInvalidOperationException()
    {
        // Arrange
        var money1 = new Money(100.00m, "USD");
        var money2 = new Money(50.00m, "EUR");

        // Act
        var act = () => money1.Add(money2);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot add money with different currencies*");
    }

    [Fact]
    public void Add_NullMoney_ThrowsArgumentNullException()
    {
        // Arrange
        var money = new Money(100.00m, "USD");

        // Act
        var act = () => money.Add(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Subtract_SameCurrency_ReturnsDifference()
    {
        // Arrange
        var money1 = new Money(100.00m, "USD");
        var money2 = new Money(30.00m, "USD");

        // Act
        var result = money1.Subtract(money2);

        // Assert
        result.Amount.Should().Be(70.00m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Subtract_DifferentCurrency_ThrowsInvalidOperationException()
    {
        // Arrange
        var money1 = new Money(100.00m, "USD");
        var money2 = new Money(50.00m, "EUR");

        // Act
        var act = () => money1.Subtract(money2);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot subtract money with different currencies*");
    }

    [Fact]
    public void Subtract_ResultNegative_ThrowsInvalidOperationException()
    {
        // Arrange
        var money1 = new Money(30.00m, "USD");
        var money2 = new Money(50.00m, "USD");

        // Act
        var act = () => money1.Subtract(money2);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Subtraction would result in a negative amount.*");
    }

    [Fact]
    public void Multiply_PositiveFactor_ReturnsProduct()
    {
        // Arrange
        var money = new Money(100.00m, "USD");

        // Act
        var result = money.Multiply(2.5m);

        // Assert
        result.Amount.Should().Be(250.00m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Multiply_ZeroFactor_ReturnsZero()
    {
        // Arrange
        var money = new Money(100.00m, "USD");

        // Act
        var result = money.Multiply(0);

        // Assert
        result.Amount.Should().Be(0);
    }

    [Fact]
    public void Multiply_NegativeFactor_ThrowsArgumentException()
    {
        // Arrange
        var money = new Money(100.00m, "USD");

        // Act
        var act = () => money.Multiply(-1);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Factor cannot be negative.*");
    }

    [Fact]
    public void PlusOperator_AddsMoney()
    {
        // Arrange
        var money1 = new Money(100.00m, "USD");
        var money2 = new Money(50.00m, "USD");

        // Act
        var result = money1 + money2;

        // Assert
        result.Amount.Should().Be(150.00m);
    }

    [Fact]
    public void MinusOperator_SubtractsMoney()
    {
        // Arrange
        var money1 = new Money(100.00m, "USD");
        var money2 = new Money(30.00m, "USD");

        // Act
        var result = money1 - money2;

        // Assert
        result.Amount.Should().Be(70.00m);
    }

    [Fact]
    public void MultiplyOperator_MoneyTimesDecimal_MultipliesMoney()
    {
        // Arrange
        var money = new Money(100.00m, "USD");

        // Act
        var result = money * 2;

        // Assert
        result.Amount.Should().Be(200.00m);
    }

    [Fact]
    public void MultiplyOperator_DecimalTimesMoney_MultipliesMoney()
    {
        // Arrange
        var money = new Money(100.00m, "USD");

        // Act
        var result = 2 * money;

        // Assert
        result.Amount.Should().Be(200.00m);
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var money = new Money(100.50m, "USD");

        // Act
        var result = money.ToString();

        // Assert
        result.Should().Be("100.50 USD");
    }

    [Fact]
    public void Equals_SameAmountAndCurrency_ReturnsTrue()
    {
        // Arrange
        var money1 = new Money(100.00m, "USD");
        var money2 = new Money(100.00m, "USD");

        // Act & Assert
        money1.Equals(money2).Should().BeTrue();
        (money1 == money2).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentAmount_ReturnsFalse()
    {
        // Arrange
        var money1 = new Money(100.00m, "USD");
        var money2 = new Money(200.00m, "USD");

        // Act & Assert
        money1.Equals(money2).Should().BeFalse();
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
    public void GetHashCode_SameValues_ReturnsSameHashCode()
    {
        // Arrange
        var money1 = new Money(100.00m, "USD");
        var money2 = new Money(100.00m, "USD");

        // Act & Assert
        money1.GetHashCode().Should().Be(money2.GetHashCode());
    }
}
