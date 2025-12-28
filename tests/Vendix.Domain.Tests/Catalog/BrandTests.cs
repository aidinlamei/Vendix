using Vendix.Domain.Catalog.Entities;
using Vendix.Domain.Catalog.ValueObjects;
using Vendix.Domain.Common;

namespace Vendix.Domain.Tests.Catalog;

/// <summary>
/// Unit tests for <see cref="Brand"/> aggregate root.
/// </summary>
public class BrandTests
{
    private static Brand CreateValidBrand(
        string name = "Test Brand",
        string slug = "test-brand",
        string? logoUrl = null)
    {
        return new Brand(name, new Slug(slug), logoUrl);
    }

    [Fact]
    public void Constructor_ValidInputs_CreatesBrand()
    {
        // Arrange
        var name = "Nike";
        var slug = new Slug("nike");
        var logoUrl = "https://example.com/nike-logo.png";

        // Act
        var brand = new Brand(name, slug, logoUrl);

        // Assert
        brand.Name.Should().Be(name);
        brand.Slug.Should().Be(slug);
        brand.LogoUrl.Should().Be(logoUrl);
        brand.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Constructor_WithoutLogoUrl_CreatesBrandWithNullLogo()
    {
        // Arrange
        var name = "Adidas";
        var slug = new Slug("adidas");

        // Act
        var brand = new Brand(name, slug);

        // Assert
        brand.Name.Should().Be(name);
        brand.Slug.Should().Be(slug);
        brand.LogoUrl.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_EmptyOrNullName_ThrowsArgumentException(string? name)
    {
        // Arrange & Act
        var act = () => new Brand(name!, new Slug("test-brand"));

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Brand name is required.*");
    }

    [Fact]
    public void Constructor_NullSlug_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new Brand("Test Brand", null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_TrimsName_WhenNameHasWhitespace()
    {
        // Arrange & Act
        var brand = new Brand("  Test Brand  ", new Slug("test-brand"));

        // Assert
        brand.Name.Should().Be("Test Brand");
    }

    [Fact]
    public void UpdateName_ValidName_UpdatesName()
    {
        // Arrange
        var brand = CreateValidBrand();
        var newName = "Updated Brand";

        // Act
        brand.UpdateName(newName);

        // Assert
        brand.Name.Should().Be(newName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void UpdateName_EmptyOrNullName_ThrowsArgumentException(string? name)
    {
        // Arrange
        var brand = CreateValidBrand();

        // Act
        var act = () => brand.UpdateName(name!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Brand name is required.*");
    }

    [Fact]
    public void UpdateSlug_ValidSlug_UpdatesSlug()
    {
        // Arrange
        var brand = CreateValidBrand();
        var newSlug = new Slug("updated-brand");

        // Act
        brand.UpdateSlug(newSlug);

        // Assert
        brand.Slug.Should().Be(newSlug);
    }

    [Fact]
    public void UpdateSlug_NullSlug_ThrowsArgumentNullException()
    {
        // Arrange
        var brand = CreateValidBrand();

        // Act
        var act = () => brand.UpdateSlug(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void UpdateLogoUrl_ValidUrl_UpdatesLogoUrl()
    {
        // Arrange
        var brand = CreateValidBrand();
        var newLogoUrl = "https://example.com/new-logo.png";

        // Act
        brand.UpdateLogoUrl(newLogoUrl);

        // Assert
        brand.LogoUrl.Should().Be(newLogoUrl);
    }

    [Fact]
    public void UpdateLogoUrl_NullUrl_SetsLogoUrlToNull()
    {
        // Arrange
        var brand = CreateValidBrand(logoUrl: "https://example.com/logo.png");

        // Act
        brand.UpdateLogoUrl(null);

        // Assert
        brand.LogoUrl.Should().BeNull();
    }

    [Fact]
    public void Brand_ImplementsIAuditableEntity()
    {
        // Arrange
        var brand = CreateValidBrand();
        var now = DateTime.UtcNow;

        // Act
        brand.CreatedAt = now;
        brand.CreatedBy = "admin";
        brand.ModifiedAt = now.AddHours(1);
        brand.ModifiedBy = "editor";

        // Assert
        brand.CreatedAt.Should().Be(now);
        brand.CreatedBy.Should().Be("admin");
        brand.ModifiedAt.Should().Be(now.AddHours(1));
        brand.ModifiedBy.Should().Be("editor");
    }

    [Fact]
    public void Brand_ImplementsISoftDelete()
    {
        // Arrange
        var brand = CreateValidBrand();
        var now = DateTime.UtcNow;

        // Act
        brand.IsDeleted = true;
        brand.DeletedAt = now;
        brand.DeletedBy = "admin";

        // Assert
        brand.IsDeleted.Should().BeTrue();
        brand.DeletedAt.Should().Be(now);
        brand.DeletedBy.Should().Be("admin");
    }

    [Fact]
    public void Brand_IsAggregateRoot()
    {
        // Arrange
        var brand = CreateValidBrand();

        // Assert
        brand.Should().BeAssignableTo<AggregateRoot>();
    }

    [Fact]
    public void Brand_HasRowVersionProperty()
    {
        // Arrange
        var brand = CreateValidBrand();

        // Assert
        // RowVersion is null for new entities and will be set by EF Core after save
        brand.RowVersion.Should().BeNull();
    }

    [Fact]
    public void Brand_DefaultIsDeletedIsFalse()
    {
        // Arrange
        var brand = CreateValidBrand();

        // Assert
        brand.IsDeleted.Should().BeFalse();
    }
}
