using Vendix.Domain.Catalog.ValueObjects;

namespace Vendix.Domain.Tests.Catalog;

/// <summary>
/// Unit tests for <see cref="Sku"/> value object.
/// </summary>
public class SkuTests
{
    [Fact]
    public void Constructor_ValidSku_CreatesSku()
    {
        // Arrange & Act
        var sku = new Sku("TEST-001");

        // Assert
        sku.Value.Should().Be("TEST-001");
    }

    [Fact]
    public void Constructor_LowercaseSku_NormalizesToUppercase()
    {
        // Arrange & Act
        var sku = new Sku("test-001");

        // Assert
        sku.Value.Should().Be("TEST-001");
    }

    [Fact]
    public void Constructor_MixedCaseSku_NormalizesToUppercase()
    {
        // Arrange & Act
        var sku = new Sku("TeSt-001-AbC");

        // Assert
        sku.Value.Should().Be("TEST-001-ABC");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Constructor_EmptyOrNullValue_ThrowsArgumentException(string? value)
    {
        // Arrange & Act
        var act = () => new Sku(value!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("SKU is required.*");
    }

    [Theory]
    [InlineData("AB")]
    [InlineData("X1")]
    public void Constructor_TooShortValue_ThrowsArgumentException(string value)
    {
        // Arrange & Act
        var act = () => new Sku(value);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage($"SKU must be at least {Sku.MinLength} characters.*");
    }

    [Fact]
    public void Constructor_TooLongValue_ThrowsArgumentException()
    {
        // Arrange
        var longValue = new string('A', Sku.MaxLength + 1);

        // Act
        var act = () => new Sku(longValue);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage($"SKU cannot exceed {Sku.MaxLength} characters.*");
    }

    [Theory]
    [InlineData("TEST@001")]
    [InlineData("TEST 001")]
    [InlineData("TEST.001")]
    [InlineData("TEST#001")]
    [InlineData("TEST_001")]
    public void Constructor_InvalidCharacters_ThrowsArgumentException(string value)
    {
        // Arrange & Act
        var act = () => new Sku(value);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("SKU must contain only letters, numbers, and hyphens.*");
    }

    [Fact]
    public void Constructor_MinLengthValue_CreatesSku()
    {
        // Arrange & Act
        var sku = new Sku("ABC");

        // Assert
        sku.Value.Should().Be("ABC");
    }

    [Fact]
    public void Constructor_MaxLengthValue_CreatesSku()
    {
        // Arrange
        var maxLengthValue = new string('A', Sku.MaxLength);

        // Act
        var sku = new Sku(maxLengthValue);

        // Assert
        sku.Value.Should().Be(maxLengthValue);
    }

    [Theory]
    [InlineData("TEST-001")]
    [InlineData("ABC123")]
    [InlineData("PROD-123-XYZ")]
    [InlineData("A1B2C3")]
    public void IsValid_ValidSkuFormat_ReturnsTrue(string value)
    {
        // Act
        var result = Sku.IsValid(value);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    [InlineData("AB")]
    [InlineData("TEST@001")]
    [InlineData("TEST 001")]
    public void IsValid_InvalidSkuFormat_ReturnsFalse(string? value)
    {
        // Act
        var result = Sku.IsValid(value);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_TooLongValue_ReturnsFalse()
    {
        // Arrange
        var longValue = new string('A', Sku.MaxLength + 1);

        // Act
        var result = Sku.IsValid(longValue);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ToString_ReturnsSKuValue()
    {
        // Arrange
        var sku = new Sku("TEST-001");

        // Act
        var result = sku.ToString();

        // Assert
        result.Should().Be("TEST-001");
    }

    [Fact]
    public void ImplicitConversionToString_ReturnsSKuValue()
    {
        // Arrange
        var sku = new Sku("TEST-001");

        // Act
        string result = sku;

        // Assert
        result.Should().Be("TEST-001");
    }

    [Fact]
    public void Equals_SameValue_ReturnsTrue()
    {
        // Arrange
        var sku1 = new Sku("TEST-001");
        var sku2 = new Sku("test-001");

        // Act & Assert
        sku1.Equals(sku2).Should().BeTrue();
        (sku1 == sku2).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentValue_ReturnsFalse()
    {
        // Arrange
        var sku1 = new Sku("TEST-001");
        var sku2 = new Sku("TEST-002");

        // Act & Assert
        sku1.Equals(sku2).Should().BeFalse();
        (sku1 != sku2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_SameValue_ReturnsSameHashCode()
    {
        // Arrange
        var sku1 = new Sku("TEST-001");
        var sku2 = new Sku("test-001");

        // Act & Assert
        sku1.GetHashCode().Should().Be(sku2.GetHashCode());
    }
}
