using Vendix.Domain.Catalog.Entities;
using Vendix.Domain.Catalog.ValueObjects;
using Vendix.Infrastructure.Persistence.Repositories;

namespace Vendix.Integration.Tests.Persistence;

/// <summary>
/// Integration tests for CategoryRepository against a real PostgreSQL database.
/// Tests verify that the repository correctly persists and retrieves categories,
/// including hierarchy relationships and translations.
/// </summary>
[Collection(nameof(DatabaseCollection))]
public class CategoryRepositoryTests
{
    private readonly DatabaseFixture _fixture;

    public CategoryRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetRootCategoriesAsync_ExcludesSubCategories()
    {
        // Arrange
        await using var writeContext = _fixture.CreateContext();
        var root = new Category("Root Category", new Slug($"root-cat-{Guid.NewGuid():N}"));
        writeContext.Categories.Add(root);
        await writeContext.SaveChangesAsync();

        var child = new Category("Child Category", new Slug($"child-cat-{Guid.NewGuid():N}"), root.Id);
        writeContext.Categories.Add(child);
        await writeContext.SaveChangesAsync();

        // Act
        await using var readContext = _fixture.CreateContext();
        var readRepository = new CategoryRepository(readContext);
        var roots = await readRepository.GetRootCategoriesAsync();

        // Assert
        roots.Should().Contain(c => c.Id == root.Id);
        roots.Should().NotContain(c => c.Id == child.Id);
    }

    [Fact]
    public async Task GetWithChildrenAsync_ReturnsParentWithSubCategoriesLoaded()
    {
        // Arrange
        await using var writeContext = _fixture.CreateContext();
        var parent = new Category("Parent Category", new Slug($"parent-cat-{Guid.NewGuid():N}"));
        writeContext.Categories.Add(parent);
        await writeContext.SaveChangesAsync();

        var child = new Category("Child Of Parent", new Slug($"child-of-parent-{Guid.NewGuid():N}"), parent.Id);
        writeContext.Categories.Add(child);
        await writeContext.SaveChangesAsync();

        // Act
        await using var readContext = _fixture.CreateContext();
        var readRepository = new CategoryRepository(readContext);
        var found = await readRepository.GetWithChildrenAsync(parent.Id);

        // Assert
        found.Should().NotBeNull();
        found!.SubCategories.Should().ContainSingle(c => c.Id == child.Id);
    }

    [Fact]
    public async Task AddTranslation_ThenReload_PersistsTranslation()
    {
        // Arrange
        await using var writeContext = _fixture.CreateContext();
        var category = new Category("Translated Category", new Slug($"translated-cat-{Guid.NewGuid():N}"));
        category.AddTranslation("fa", "دسته‌بندی ترجمه‌شده", "توضیحات نمونه");
        writeContext.Categories.Add(category);
        await writeContext.SaveChangesAsync();

        // Act
        await using var readContext = _fixture.CreateContext();
        var readRepository = new CategoryRepository(readContext);
        var found = await readRepository.GetBySlugAsync(category.Slug.Value);

        // Assert
        found.Should().NotBeNull();
        found!.Translations.Should().ContainSingle(t => t.LanguageCode == "fa" && t.Name == "دسته‌بندی ترجمه‌شده");
    }
}
