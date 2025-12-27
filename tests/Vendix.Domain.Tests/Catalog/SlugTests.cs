using Vendix.Domain.Catalog.ValueObjects;

namespace Vendix.Domain.Tests.Catalog;

/// <summary>
/// Unit tests for <see cref="Slug"/> value object.
/// </summary>
public class SlugTests
{
    [Fact]
    public void Constructor_ValidSlug_CreatesSlug()
    {
        // Arrange & Act
        var slug = new Slug("test-product");

        // Assert
        slug.Value.Should().Be("test-product");
    }

    [Fact]
    public void Constructor_UppercaseSlug_NormalizesToLowercase()
    {
        // Arrange & Act
        var slug = new Slug("TEST-PRODUCT");

        // Assert
        slug.Value.Should().Be("test-product");
    }

    [Fact]
    public void Constructor_MixedCaseSlug_NormalizesToLowercase()
    {
        // Arrange & Act
        var slug = new Slug("Test-Product-123");

        // Assert
        slug.Value.Should().Be("test-product-123");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Constructor_EmptyOrNullValue_ThrowsArgumentException(string? value)
    {
        // Arrange & Act
        var act = () => new Slug(value!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Slug is required.*");
    }

    [Theory]
    [InlineData("a")]
    public void Constructor_TooShortValue_ThrowsArgumentException(string value)
    {
        // Arrange & Act
        var act = () => new Slug(value);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage($"Slug must be at least {Slug.MinLength} characters.*");
    }

    [Fact]
    public void Constructor_TooLongValue_ThrowsArgumentException()
    {
        // Arrange
        var longValue = new string('a', Slug.MaxLength + 1);

        // Act
        var act = () => new Slug(longValue);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage($"Slug cannot exceed {Slug.MaxLength} characters.*");
    }

    [Theory]
    [InlineData("test_product")]
    [InlineData("test@product")]
    [InlineData("test product")]
    [InlineData("test.product")]
    public void Constructor_InvalidCharacters_ThrowsArgumentException(string value)
    {
        // Arrange & Act
        var act = () => new Slug(value);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Slug must contain only lowercase letters, numbers, and hyphens.*");
    }

    [Theory]
    [InlineData("-test-product")]
    [InlineData("test-product-")]
    [InlineData("-test-product-")]
    public void Constructor_StartsOrEndsWithHyphen_ThrowsArgumentException(string value)
    {
        // Arrange & Act
        var act = () => new Slug(value);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Slug cannot start or end with a hyphen.*");
    }

    [Theory]
    [InlineData("test--product")]
    [InlineData("test---product")]
    [InlineData("a--b--c")]
    public void Constructor_ConsecutiveHyphens_ThrowsArgumentException(string value)
    {
        // Arrange & Act
        var act = () => new Slug(value);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Slug cannot contain consecutive hyphens.*");
    }

    [Fact]
    public void Constructor_MinLengthValue_CreatesSlug()
    {
        // Arrange & Act
        var slug = new Slug("ab");

        // Assert
        slug.Value.Should().Be("ab");
    }

    [Fact]
    public void FromText_ValidText_CreatesSlug()
    {
        // Arrange & Act
        var slug = Slug.FromText("Test Product Name");

        // Assert
        slug.Value.Should().Be("test-product-name");
    }

    [Fact]
    public void FromText_TextWithUnderscores_ReplacesWithHyphens()
    {
        // Arrange & Act
        var slug = Slug.FromText("test_product_name");

        // Assert
        slug.Value.Should().Be("test-product-name");
    }

    [Fact]
    public void FromText_TextWithSpecialCharacters_RemovesInvalidChars()
    {
        // Arrange & Act
        var slug = Slug.FromText("Test! Product@ #123");

        // Assert
        slug.Value.Should().Be("test-product-123");
    }

    [Fact]
    public void FromText_TextWithMultipleSpaces_CollapseToSingleHyphen()
    {
        // Arrange & Act
        var slug = Slug.FromText("Test   Product   Name");

        // Assert
        slug.Value.Should().Be("test-product-name");
    }

    [Fact]
    public void FromText_TextWithLeadingTrailingSpaces_TrimsAndCreatesSlug()
    {
        // Arrange & Act
        var slug = Slug.FromText("  Test Product  ");

        // Assert
        slug.Value.Should().Be("test-product");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void FromText_EmptyOrNullText_ThrowsArgumentException(string? text)
    {
        // Arrange & Act
        var act = () => Slug.FromText(text!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Text is required.*");
    }

    [Fact]
    public void FromText_TextResultingInTooShortSlug_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => Slug.FromText("@");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*too short*");
    }

    [Theory]
    [InlineData("test-product")]
    [InlineData("abc123")]
    [InlineData("my-awesome-product-2024")]
    public void IsValid_ValidSlugFormat_ReturnsTrue(string value)
    {
        // Act
        var result = Slug.IsValid(value);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    [InlineData("a")]
    [InlineData("test_product")]
    [InlineData("-test-product")]
    [InlineData("test-product-")]
    [InlineData("test--product")]
    public void IsValid_InvalidSlugFormat_ReturnsFalse(string? value)
    {
        // Act
        var result = Slug.IsValid(value);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_TooLongValue_ReturnsFalse()
    {
        // Arrange
        var longValue = new string('a', Slug.MaxLength + 1);

        // Act
        var result = Slug.IsValid(longValue);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ToString_ReturnsSlugValue()
    {
        // Arrange
        var slug = new Slug("test-product");

        // Act
        var result = slug.ToString();

        // Assert
        result.Should().Be("test-product");
    }

    [Fact]
    public void ImplicitConversionToString_ReturnsSlugValue()
    {
        // Arrange
        var slug = new Slug("test-product");

        // Act
        string result = slug;

        // Assert
        result.Should().Be("test-product");
    }

    [Fact]
    public void Equals_SameValue_ReturnsTrue()
    {
        // Arrange
        var slug1 = new Slug("test-product");
        var slug2 = new Slug("TEST-PRODUCT");

        // Act & Assert
        slug1.Equals(slug2).Should().BeTrue();
        (slug1 == slug2).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentValue_ReturnsFalse()
    {
        // Arrange
        var slug1 = new Slug("test-product");
        var slug2 = new Slug("other-product");

        // Act & Assert
        slug1.Equals(slug2).Should().BeFalse();
        (slug1 != slug2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_SameValue_ReturnsSameHashCode()
    {
        // Arrange
        var slug1 = new Slug("test-product");
        var slug2 = new Slug("TEST-PRODUCT");

        // Act & Assert
        slug1.GetHashCode().Should().Be(slug2.GetHashCode());
    }
}
