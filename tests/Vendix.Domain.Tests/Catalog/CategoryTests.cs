using Vendix.Domain.Catalog.Entities;
using Vendix.Domain.Catalog.ValueObjects;

namespace Vendix.Domain.Tests.Catalog;

/// <summary>
/// Unit tests for <see cref="Category"/> aggregate root.
/// </summary>
public class CategoryTests
{
    private static Category CreateValidCategory(
        string name = "Test Category",
        string slug = "test-category",
        Guid? parentCategoryId = null)
    {
        return new Category(name, new Slug(slug), parentCategoryId);
    }

    [Fact]
    public void Constructor_ValidInputs_CreatesCategory()
    {
        // Arrange
        var name = "Electronics";
        var slug = new Slug("electronics");

        // Act
        var category = new Category(name, slug);

        // Assert
        category.Name.Should().Be(name);
        category.Slug.Should().Be(slug);
        category.ParentCategoryId.Should().BeNull();
        category.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Constructor_WithParentCategoryId_SetsParentCategoryId()
    {
        // Arrange
        var parentId = Guid.NewGuid();

        // Act
        var category = new Category("Smartphones", new Slug("smartphones"), parentId);

        // Assert
        category.ParentCategoryId.Should().Be(parentId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Constructor_EmptyOrNullName_ThrowsArgumentException(string? name)
    {
        // Arrange & Act
        var act = () => new Category(name!, new Slug("test"));

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Category name is required.*");
    }

    [Fact]
    public void Constructor_NullSlug_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new Category("Test Category", null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void UpdateName_ValidName_UpdatesName()
    {
        // Arrange
        var category = CreateValidCategory();
        var newName = "Updated Category";

        // Act
        category.UpdateName(newName);

        // Assert
        category.Name.Should().Be(newName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void UpdateName_EmptyOrNullName_ThrowsArgumentException(string? name)
    {
        // Arrange
        var category = CreateValidCategory();

        // Act
        var act = () => category.UpdateName(name!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Category name is required.*");
    }

    [Fact]
    public void UpdateSlug_ValidSlug_UpdatesSlug()
    {
        // Arrange
        var category = CreateValidCategory();
        var newSlug = new Slug("updated-category");

        // Act
        category.UpdateSlug(newSlug);

        // Assert
        category.Slug.Should().Be(newSlug);
    }

    [Fact]
    public void UpdateSlug_NullSlug_ThrowsArgumentNullException()
    {
        // Arrange
        var category = CreateValidCategory();

        // Act
        var act = () => category.UpdateSlug(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void SetParentCategory_ValidId_SetsParentCategoryId()
    {
        // Arrange
        var category = CreateValidCategory();
        var parentId = Guid.NewGuid();

        // Act
        category.SetParentCategory(parentId);

        // Assert
        category.ParentCategoryId.Should().Be(parentId);
    }

    [Fact]
    public void SetParentCategory_Null_RemovesParent()
    {
        // Arrange
        var category = CreateValidCategory(parentCategoryId: Guid.NewGuid());

        // Act
        category.SetParentCategory(null);

        // Assert
        category.ParentCategoryId.Should().BeNull();
    }

    [Fact]
    public void SetParentCategory_SelfAsParent_ThrowsInvalidOperationException()
    {
        // Arrange
        var category = CreateValidCategory();

        // Act
        var act = () => category.SetParentCategory(category.Id);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*cannot be its own parent*");
    }

    [Fact]
    public void AddTranslation_ValidTranslation_AddsToCollection()
    {
        // Arrange
        var category = CreateValidCategory();

        // Act
        var translation = category.AddTranslation("fa", "دسته‌بندی آزمایشی", "توضیحات دسته‌بندی");

        // Assert
        category.Translations.Should().HaveCount(1);
        category.Translations.Should().Contain(translation);
        translation.LanguageCode.Should().Be("fa");
        translation.Name.Should().Be("دسته‌بندی آزمایشی");
        translation.Description.Should().Be("توضیحات دسته‌بندی");
    }

    [Fact]
    public void AddTranslation_WithoutDescription_AddsTranslation()
    {
        // Arrange
        var category = CreateValidCategory();

        // Act
        var translation = category.AddTranslation("de", "Testkategorie");

        // Assert
        category.Translations.Should().HaveCount(1);
        translation.Description.Should().BeNull();
    }

    [Fact]
    public void AddTranslation_UppercaseLanguageCode_NormalizesToLowercase()
    {
        // Arrange
        var category = CreateValidCategory();

        // Act
        var translation = category.AddTranslation("FA", "دسته‌بندی");

        // Assert
        translation.LanguageCode.Should().Be("fa");
    }

    [Fact]
    public void AddTranslation_DuplicateLanguage_ThrowsInvalidOperationException()
    {
        // Arrange
        var category = CreateValidCategory();
        category.AddTranslation("fa", "دسته‌بندی اول");

        // Act
        var act = () => category.AddTranslation("FA", "دسته‌بندی دوم");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*translation for language*already exists*");
    }

    [Fact]
    public void GetTranslation_ExistingLanguage_ReturnsTranslation()
    {
        // Arrange
        var category = CreateValidCategory();
        var addedTranslation = category.AddTranslation("fa", "دسته‌بندی");

        // Act
        var translation = category.GetTranslation("fa");

        // Assert
        translation.Should().Be(addedTranslation);
    }

    [Fact]
    public void GetTranslation_NonExistingLanguage_ReturnsNull()
    {
        // Arrange
        var category = CreateValidCategory();
        category.AddTranslation("fa", "دسته‌بندی");

        // Act
        var translation = category.GetTranslation("de");

        // Assert
        translation.Should().BeNull();
    }

    [Fact]
    public void GetTranslation_UppercaseLanguageCode_FindsTranslation()
    {
        // Arrange
        var category = CreateValidCategory();
        category.AddTranslation("fa", "دسته‌بندی");

        // Act
        var translation = category.GetTranslation("FA");

        // Assert
        translation.Should().NotBeNull();
        translation!.Name.Should().Be("دسته‌بندی");
    }

    [Fact]
    public void GetName_ExistingTranslation_ReturnsTranslatedName()
    {
        // Arrange
        var category = CreateValidCategory(name: "Test Category");
        category.AddTranslation("fa", "دسته‌بندی آزمایشی");

        // Act
        var name = category.GetName("fa");

        // Assert
        name.Should().Be("دسته‌بندی آزمایشی");
    }

    [Fact]
    public void GetName_NonExistingTranslation_ReturnsDefaultName()
    {
        // Arrange
        var category = CreateValidCategory(name: "Test Category");

        // Act
        var name = category.GetName("de");

        // Assert
        name.Should().Be("Test Category");
    }

    [Fact]
    public void RemoveTranslation_ExistingLanguage_RemovesAndReturnsTrue()
    {
        // Arrange
        var category = CreateValidCategory();
        category.AddTranslation("fa", "دسته‌بندی");

        // Act
        var result = category.RemoveTranslation("fa");

        // Assert
        result.Should().BeTrue();
        category.Translations.Should().BeEmpty();
    }

    [Fact]
    public void RemoveTranslation_NonExistingLanguage_ReturnsFalse()
    {
        // Arrange
        var category = CreateValidCategory();
        category.AddTranslation("fa", "دسته‌بندی");

        // Act
        var result = category.RemoveTranslation("de");

        // Assert
        result.Should().BeFalse();
        category.Translations.Should().HaveCount(1);
    }

    [Fact]
    public void RemoveTranslation_UppercaseLanguageCode_RemovesTranslation()
    {
        // Arrange
        var category = CreateValidCategory();
        category.AddTranslation("fa", "دسته‌بندی");

        // Act
        var result = category.RemoveTranslation("FA");

        // Assert
        result.Should().BeTrue();
        category.Translations.Should().BeEmpty();
    }

    [Fact]
    public void Category_ImplementsIAuditableEntity()
    {
        // Arrange
        var category = CreateValidCategory();
        var now = DateTime.UtcNow;

        // Act
        category.CreatedAt = now;
        category.CreatedBy = "admin";
        category.ModifiedAt = now.AddHours(1);
        category.ModifiedBy = "editor";

        // Assert
        category.CreatedAt.Should().Be(now);
        category.CreatedBy.Should().Be("admin");
        category.ModifiedAt.Should().Be(now.AddHours(1));
        category.ModifiedBy.Should().Be("editor");
    }

    [Fact]
    public void Category_ImplementsISoftDelete()
    {
        // Arrange
        var category = CreateValidCategory();
        var now = DateTime.UtcNow;

        // Act
        category.IsDeleted = true;
        category.DeletedAt = now;
        category.DeletedBy = "admin";

        // Assert
        category.IsDeleted.Should().BeTrue();
        category.DeletedAt.Should().Be(now);
        category.DeletedBy.Should().Be("admin");
    }

    [Fact]
    public void Category_TranslationsStartsEmpty()
    {
        // Arrange & Act
        var category = CreateValidCategory();

        // Assert
        category.Translations.Should().BeEmpty();
    }

    [Fact]
    public void Category_SubCategoriesStartsEmpty()
    {
        // Arrange & Act
        var category = CreateValidCategory();

        // Assert
        category.SubCategories.Should().BeEmpty();
    }

    [Fact]
    public void Category_ProductsStartsEmpty()
    {
        // Arrange & Act
        var category = CreateValidCategory();

        // Assert
        category.Products.Should().BeEmpty();
    }
}
