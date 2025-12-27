using Vendix.Domain.Catalog.Entities;
using Vendix.Domain.Catalog.Enums;
using Vendix.Domain.Catalog.ValueObjects;

namespace Vendix.Domain.Tests.Catalog;

/// <summary>
/// Unit tests for <see cref="Product"/> aggregate root.
/// </summary>
public class ProductTests
{
    private static Product CreateValidProduct(
        string name = "Test Product",
        string sku = "TEST-001",
        string slug = "test-product",
        decimal price = 99.99m,
        string currency = "USD",
        ProductType productType = ProductType.Physical)
    {
        return new Product(
            name,
            new Sku(sku),
            new Slug(slug),
            new Money(price, currency),
            productType);
    }

    [Fact]
    public void Constructor_ValidInputs_CreatesProduct()
    {
        // Arrange
        var name = "Test Product";
        var sku = new Sku("TEST-001");
        var slug = new Slug("test-product");
        var price = new Money(99.99m, "USD");
        var productType = ProductType.Physical;

        // Act
        var product = new Product(name, sku, slug, price, productType);

        // Assert
        product.Name.Should().Be(name);
        product.Sku.Should().Be(sku);
        product.Slug.Should().Be(slug);
        product.Price.Should().Be(price);
        product.ProductType.Should().Be(productType);
        product.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Constructor_WithOptionalParameters_SetsAllValues()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var brandId = Guid.NewGuid();
        var description = "A great product";

        // Act
        var product = new Product(
            "Test Product",
            new Sku("TEST-001"),
            new Slug("test-product"),
            new Money(99.99m, "USD"),
            ProductType.Digital,
            description,
            categoryId,
            brandId);

        // Assert
        product.Description.Should().Be(description);
        product.CategoryId.Should().Be(categoryId);
        product.BrandId.Should().Be(brandId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Constructor_EmptyOrNullName_ThrowsArgumentException(string? name)
    {
        // Arrange & Act
        var act = () => new Product(
            name!,
            new Sku("TEST-001"),
            new Slug("test-product"),
            new Money(99.99m, "USD"),
            ProductType.Physical);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Product name is required.*");
    }

    [Fact]
    public void Constructor_NullSku_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new Product(
            "Test Product",
            null!,
            new Slug("test-product"),
            new Money(99.99m, "USD"),
            ProductType.Physical);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullSlug_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new Product(
            "Test Product",
            new Sku("TEST-001"),
            null!,
            new Money(99.99m, "USD"),
            ProductType.Physical);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullPrice_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new Product(
            "Test Product",
            new Sku("TEST-001"),
            new Slug("test-product"),
            null!,
            ProductType.Physical);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void UpdatePrice_ValidPrice_UpdatesPrice()
    {
        // Arrange
        var product = CreateValidProduct();
        var newPrice = new Money(149.99m, "USD");

        // Act
        product.UpdatePrice(newPrice);

        // Assert
        product.Price.Should().Be(newPrice);
    }

    [Fact]
    public void UpdatePrice_NullPrice_ThrowsArgumentNullException()
    {
        // Arrange
        var product = CreateValidProduct();

        // Act
        var act = () => product.UpdatePrice(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddVariant_ValidVariant_AddsToCollection()
    {
        // Arrange
        var product = CreateValidProduct();
        var variantSku = new Sku("TEST-001-L");
        var priceAdjustment = new Money(10.00m, "USD");

        // Act
        var variant = product.AddVariant(variantSku, "Large", priceAdjustment, 100);

        // Assert
        product.Variants.Should().HaveCount(1);
        product.Variants.Should().Contain(variant);
        variant.Sku.Should().Be(variantSku);
        variant.Name.Should().Be("Large");
        variant.PriceAdjustment.Should().Be(priceAdjustment);
        variant.StockQuantity.Should().Be(100);
    }

    [Fact]
    public void AddVariant_DuplicateSku_ThrowsInvalidOperationException()
    {
        // Arrange
        var product = CreateValidProduct();
        var variantSku = new Sku("TEST-001-L");
        var priceAdjustment = new Money(10.00m, "USD");
        product.AddVariant(variantSku, "Large", priceAdjustment);

        // Act
        var act = () => product.AddVariant(variantSku, "Extra Large", priceAdjustment);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*variant with SKU*already exists*");
    }

    [Fact]
    public void RemoveVariant_ExistingVariant_RemovesAndReturnsTrue()
    {
        // Arrange
        var product = CreateValidProduct();
        var variant = product.AddVariant(new Sku("TEST-001-L"), "Large", new Money(10.00m, "USD"));

        // Act
        var result = product.RemoveVariant(variant.Id);

        // Assert
        result.Should().BeTrue();
        product.Variants.Should().BeEmpty();
    }

    [Fact]
    public void RemoveVariant_NonExistingVariant_ReturnsFalse()
    {
        // Arrange
        var product = CreateValidProduct();

        // Act
        var result = product.RemoveVariant(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void AddSpecification_ValidSpecification_AddsToCollection()
    {
        // Arrange
        var product = CreateValidProduct();

        // Act
        var spec = product.AddSpecification("Weight", "500g");

        // Assert
        product.Specifications.Should().HaveCount(1);
        product.Specifications.Should().Contain(spec);
        spec.Name.Should().Be("Weight");
        spec.Value.Should().Be("500g");
    }

    [Fact]
    public void AddSpecification_DuplicateName_ThrowsInvalidOperationException()
    {
        // Arrange
        var product = CreateValidProduct();
        product.AddSpecification("Weight", "500g");

        // Act
        var act = () => product.AddSpecification("weight", "600g");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*specification with name*already exists*");
    }

    [Fact]
    public void AddTranslation_ValidTranslation_AddsToCollection()
    {
        // Arrange
        var product = CreateValidProduct();

        // Act
        var translation = product.AddTranslation("fa", "محصول تست", "توضیحات محصول");

        // Assert
        product.Translations.Should().HaveCount(1);
        product.Translations.Should().Contain(translation);
        translation.LanguageCode.Should().Be("fa");
        translation.Title.Should().Be("محصول تست");
        translation.Description.Should().Be("توضیحات محصول");
    }

    [Fact]
    public void AddTranslation_DuplicateLanguage_ThrowsInvalidOperationException()
    {
        // Arrange
        var product = CreateValidProduct();
        product.AddTranslation("fa", "محصول تست");

        // Act
        var act = () => product.AddTranslation("FA", "محصول دیگر");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*translation for language*already exists*");
    }

    [Fact]
    public void GetTitle_ExistingTranslation_ReturnsTranslatedTitle()
    {
        // Arrange
        var product = CreateValidProduct(name: "Test Product");
        product.AddTranslation("fa", "محصول تست");

        // Act
        var title = product.GetTitle("fa");

        // Assert
        title.Should().Be("محصول تست");
    }

    [Fact]
    public void GetTitle_NonExistingTranslation_ReturnsDefaultName()
    {
        // Arrange
        var product = CreateValidProduct(name: "Test Product");

        // Act
        var title = product.GetTitle("de");

        // Assert
        title.Should().Be("Test Product");
    }

    [Fact]
    public void AddImage_FirstImage_SetsAsMain()
    {
        // Arrange
        var product = CreateValidProduct();

        // Act
        var image = product.AddImage("https://example.com/image1.jpg", "Product Image");

        // Assert
        product.Images.Should().HaveCount(1);
        image.IsMain.Should().BeTrue();
        product.GetMainImage().Should().Be(image);
    }

    [Fact]
    public void AddImage_WithIsMainTrue_UnsetsExistingMain()
    {
        // Arrange
        var product = CreateValidProduct();
        var image1 = product.AddImage("https://example.com/image1.jpg");

        // Act
        var image2 = product.AddImage("https://example.com/image2.jpg", isMain: true);

        // Assert
        product.Images.Should().HaveCount(2);
        image1.IsMain.Should().BeFalse();
        image2.IsMain.Should().BeTrue();
        product.GetMainImage().Should().Be(image2);
    }

    [Fact]
    public void RemoveImage_MainImage_SetsNextAsMain()
    {
        // Arrange
        var product = CreateValidProduct();
        var image1 = product.AddImage("https://example.com/image1.jpg");
        var image2 = product.AddImage("https://example.com/image2.jpg");

        // Act
        product.RemoveImage(image1.Id);

        // Assert
        product.Images.Should().HaveCount(1);
        image2.IsMain.Should().BeTrue();
    }

    [Fact]
    public void SetMainImage_ExistingImage_UpdatesMainImage()
    {
        // Arrange
        var product = CreateValidProduct();
        var image1 = product.AddImage("https://example.com/image1.jpg");
        var image2 = product.AddImage("https://example.com/image2.jpg");

        // Act
        product.SetMainImage(image2.Id);

        // Assert
        image1.IsMain.Should().BeFalse();
        image2.IsMain.Should().BeTrue();
    }

    [Fact]
    public void SetMainImage_NonExistingImage_ThrowsInvalidOperationException()
    {
        // Arrange
        var product = CreateValidProduct();
        product.AddImage("https://example.com/image1.jpg");

        // Act
        var act = () => product.SetMainImage(Guid.NewGuid());

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public void AssignToCategory_ValidCategoryId_UpdatesCategoryId()
    {
        // Arrange
        var product = CreateValidProduct();
        var categoryId = Guid.NewGuid();

        // Act
        product.AssignToCategory(categoryId);

        // Assert
        product.CategoryId.Should().Be(categoryId);
    }

    [Fact]
    public void AssignToCategory_Null_RemovesCategoryAssignment()
    {
        // Arrange
        var product = CreateValidProduct();
        product.AssignToCategory(Guid.NewGuid());

        // Act
        product.AssignToCategory(null);

        // Assert
        product.CategoryId.Should().BeNull();
    }

    [Fact]
    public void Product_ImplementsIAuditableEntity()
    {
        // Arrange
        var product = CreateValidProduct();
        var now = DateTime.UtcNow;

        // Act
        product.CreatedAt = now;
        product.CreatedBy = "admin";
        product.ModifiedAt = now.AddHours(1);
        product.ModifiedBy = "editor";

        // Assert
        product.CreatedAt.Should().Be(now);
        product.CreatedBy.Should().Be("admin");
        product.ModifiedAt.Should().Be(now.AddHours(1));
        product.ModifiedBy.Should().Be("editor");
    }

    [Fact]
    public void Product_ImplementsISoftDelete()
    {
        // Arrange
        var product = CreateValidProduct();
        var now = DateTime.UtcNow;

        // Act
        product.IsDeleted = true;
        product.DeletedAt = now;
        product.DeletedBy = "admin";

        // Assert
        product.IsDeleted.Should().BeTrue();
        product.DeletedAt.Should().Be(now);
        product.DeletedBy.Should().Be("admin");
    }
}
