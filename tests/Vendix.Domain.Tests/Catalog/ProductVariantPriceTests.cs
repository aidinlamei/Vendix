using Vendix.Domain.Catalog.Entities;
using Vendix.Domain.Catalog.ValueObjects;

namespace Vendix.Domain.Tests.Catalog;

/// <summary>
/// Unit tests for <see cref="ProductVariant"/> focusing on GetFinalPriceWithInfo.
/// </summary>
public class ProductVariantPriceTests
{
    private static ProductVariant CreateVariant(
        decimal priceAdjustment = 10.00m,
        string currency = "USD")
    {
        return new ProductVariant(
            Guid.NewGuid(),
            new Sku("VAR-001"),
            "Test Variant",
            priceAdjustment,
            currency,
            100);
    }

    [Fact]
    public void GetFinalPriceWithInfo_PositiveAdjustment_ReturnsCorrectPrice()
    {
        // Arrange
        var variant = CreateVariant(priceAdjustment: 20.00m);
        var basePrice = new Money(100.00m, "USD");

        // Act
        var result = variant.GetFinalPriceWithInfo(basePrice);

        // Assert
        result.Price.Amount.Should().Be(120.00m);
        result.WasClampedToZero.Should().BeFalse();
    }

    [Fact]
    public void GetFinalPriceWithInfo_NegativeAdjustment_ReturnsDiscountedPrice()
    {
        // Arrange
        var variant = CreateVariant(priceAdjustment: -20.00m);
        var basePrice = new Money(100.00m, "USD");

        // Act
        var result = variant.GetFinalPriceWithInfo(basePrice);

        // Assert
        result.Price.Amount.Should().Be(80.00m);
        result.WasClampedToZero.Should().BeFalse();
    }

    [Fact]
    public void GetFinalPriceWithInfo_ZeroAdjustment_ReturnsSamePrice()
    {
        // Arrange
        var variant = CreateVariant(priceAdjustment: 0m);
        var basePrice = new Money(100.00m, "USD");

        // Act
        var result = variant.GetFinalPriceWithInfo(basePrice);

        // Assert
        result.Price.Amount.Should().Be(100.00m);
        result.WasClampedToZero.Should().BeFalse();
    }

    [Fact]
    public void GetFinalPriceWithInfo_NegativeResult_ClampedToZero()
    {
        // Arrange
        var variant = CreateVariant(priceAdjustment: -150.00m);
        var basePrice = new Money(100.00m, "USD");

        // Act
        var result = variant.GetFinalPriceWithInfo(basePrice);

        // Assert
        result.Price.Amount.Should().Be(0m);
        result.WasClampedToZero.Should().BeTrue();
    }

    [Fact]
    public void GetFinalPriceWithInfo_ExactlyZeroResult_NotClamped()
    {
        // Arrange
        var variant = CreateVariant(priceAdjustment: -100.00m);
        var basePrice = new Money(100.00m, "USD");

        // Act
        var result = variant.GetFinalPriceWithInfo(basePrice);

        // Assert
        result.Price.Amount.Should().Be(0m);
        result.WasClampedToZero.Should().BeFalse();
    }

    [Fact]
    public void GetFinalPriceWithInfo_DifferentCurrencies_ThrowsInvalidOperationException()
    {
        // Arrange
        var variant = CreateVariant(currency: "USD");
        var basePrice = new Money(100.00m, "EUR");

        // Act
        var act = () => variant.GetFinalPriceWithInfo(basePrice);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*currency*does not match*");
    }

    [Fact]
    public void GetFinalPriceWithInfo_NullBasePrice_ThrowsArgumentNullException()
    {
        // Arrange
        var variant = CreateVariant();

        // Act
        var act = () => variant.GetFinalPriceWithInfo(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetFinalPrice_BackwardCompatibility_ReturnsSameAsGetFinalPriceWithInfo()
    {
        // Arrange
        var variant = CreateVariant(priceAdjustment: -150.00m);
        var basePrice = new Money(100.00m, "USD");

        // Act
        var legacyResult = variant.GetFinalPrice(basePrice);
        var newResult = variant.GetFinalPriceWithInfo(basePrice);

        // Assert
        legacyResult.Amount.Should().Be(newResult.Price.Amount);
        legacyResult.Currency.Should().Be(newResult.Price.Currency);
    }

    [Fact]
    public void GetFinalPriceWithInfo_PreservesCurrency()
    {
        // Arrange
        var variant = CreateVariant(priceAdjustment: 10.00m, currency: "EUR");
        var basePrice = new Money(100.00m, "EUR");

        // Act
        var result = variant.GetFinalPriceWithInfo(basePrice);

        // Assert
        result.Price.Currency.Should().Be("EUR");
    }

    [Theory]
    [InlineData(100.00, 10.00, 110.00, false)]
    [InlineData(100.00, -10.00, 90.00, false)]
    [InlineData(50.00, -60.00, 0.00, true)]
    [InlineData(0.00, 10.00, 10.00, false)]
    [InlineData(0.00, -10.00, 0.00, true)]
    public void GetFinalPriceWithInfo_VariousScenarios_ReturnsCorrectResults(
        decimal baseAmount,
        decimal adjustment,
        decimal expectedAmount,
        bool expectedClamped)
    {
        // Arrange
        var variant = CreateVariant(priceAdjustment: adjustment);
        var basePrice = new Money(baseAmount, "USD");

        // Act
        var result = variant.GetFinalPriceWithInfo(basePrice);

        // Assert
        result.Price.Amount.Should().Be(expectedAmount);
        result.WasClampedToZero.Should().Be(expectedClamped);
    }
}
