using Vendix.Application.Catalog.Commands;
using Vendix.Domain.Catalog.Enums;

namespace Vendix.Application.Tests.Catalog.Commands;

/// <summary>
/// Unit tests for <see cref="CreateProductCommandValidator"/>.
/// </summary>
public class CreateProductCommandValidatorTests
{
    private readonly CreateProductCommandValidator _validator = new();

    private static CreateProductCommand CreateValidCommand() => new(
        Name: "Test Product",
        Sku: "TEST-001",
        Slug: "test-product",
        Price: 99.99m,
        Currency: "USD",
        ProductType: ProductType.Physical);

    [Fact]
    public async Task Validate_ValidCommand_ShouldPass()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Validate_EmptyName_ShouldFail(string? name)
    {
        // Arrange
        var command = CreateValidCommand() with { Name = name! };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CreateProductCommand.Name));
    }

    [Fact]
    public async Task Validate_NameTooLong_ShouldFail()
    {
        // Arrange
        var command = CreateValidCommand() with { Name = new string('a', 201) };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CreateProductCommand.Name));
    }

    [Theory]
    [InlineData("")]
    [InlineData("AB")] // Too short
    [InlineData("A")] // Too short
    public async Task Validate_InvalidSkuFormat_ShouldFail(string sku)
    {
        // Arrange
        var command = CreateValidCommand() with { Sku = sku };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CreateProductCommand.Sku));
    }

    [Theory]
    [InlineData("TEST@001")] // Invalid character
    [InlineData("TEST 001")] // Space
    public async Task Validate_SkuWithInvalidCharacters_ShouldFail(string sku)
    {
        // Arrange
        var command = CreateValidCommand() with { Sku = sku };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CreateProductCommand.Sku));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task Validate_NegativeOrZeroPrice_ShouldFail(decimal price)
    {
        // Arrange
        var command = CreateValidCommand() with { Price = price };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CreateProductCommand.Price));
    }

    [Theory]
    [InlineData("")]
    [InlineData("US")] // Too short
    [InlineData("USDD")] // Too long
    public async Task Validate_InvalidCurrency_ShouldFail(string currency)
    {
        // Arrange
        var command = CreateValidCommand() with { Currency = currency };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CreateProductCommand.Currency));
    }

    [Theory]
    [InlineData("a")] // Too short
    public async Task Validate_InvalidSlug_ShouldFail(string slug)
    {
        // Arrange
        var command = CreateValidCommand() with { Slug = slug };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CreateProductCommand.Slug));
    }

    [Fact]
    public async Task Validate_DescriptionTooLong_ShouldFail()
    {
        // Arrange
        var command = CreateValidCommand() with { Description = new string('a', 4001) };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CreateProductCommand.Description));
    }

    [Fact]
    public async Task Validate_ValidDescription_ShouldPass()
    {
        // Arrange
        var command = CreateValidCommand() with { Description = "A valid product description" };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_NullDescription_ShouldPass()
    {
        // Arrange
        var command = CreateValidCommand() with { Description = null };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
