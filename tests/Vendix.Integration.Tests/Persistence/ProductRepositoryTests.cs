using Vendix.Domain.Catalog.Entities;
using Vendix.Domain.Catalog.Enums;
using Vendix.Domain.Catalog.ValueObjects;
using Vendix.Infrastructure.Persistence.Repositories;

namespace Vendix.Integration.Tests.Persistence;

/// <summary>
/// Integration tests for ProductRepository against a real PostgreSQL database.
/// Tests verify that the repository correctly persists and retrieves products,
/// including filtering and price mutations.
/// </summary>
[Collection(nameof(DatabaseCollection))]
public class ProductRepositoryTests
{
    private readonly DatabaseFixture _fixture;

    public ProductRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    private static string UniqueSku(string prefix) =>
        $"{prefix}-{Guid.NewGuid():N}"[..16].ToUpperInvariant();

    [Fact]
    public async Task AddAsync_ThenGetById_ReturnsPersistedProductWithCorrectPrice()
    {
        // Arrange
        await using var writeContext = _fixture.CreateContext();
        var writeRepository = new ProductRepository(writeContext);
        var product = new Product(
            "Integration Test Product",
            new Sku(UniqueSku("ITP")),
            new Slug($"integration-test-product-{Guid.NewGuid():N}"),
            new Money(49.99m, "USD"),
            ProductType.Physical);

        // Act
        await writeRepository.AddAsync(product);
        await writeContext.SaveChangesAsync();

        await using var readContext = _fixture.CreateContext();
        var readRepository = new ProductRepository(readContext);
        var found = await readRepository.GetByIdAsync(product.Id);

        // Assert
        found.Should().NotBeNull();
        found!.Name.Should().Be("Integration Test Product");
        found.Price.Amount.Should().Be(49.99m);
        found.Price.Currency.Should().Be("USD");
    }

    [Fact]
    public async Task GetBySlugAsync_DifferentCase_ReturnsProduct()
    {
        // Arrange
        await using var writeContext = _fixture.CreateContext();
        var writeRepository = new ProductRepository(writeContext);
        var slug = $"case-test-{Guid.NewGuid():N}";
        var product = new Product(
            "Case Test Product",
            new Sku(UniqueSku("CTP")),
            new Slug(slug),
            new Money(19.99m, "USD"),
            ProductType.Physical);

        await writeRepository.AddAsync(product);
        await writeContext.SaveChangesAsync();

        // Act
        await using var readContext = _fixture.CreateContext();
        var readRepository = new ProductRepository(readContext);
        var found = await readRepository.GetBySlugAsync(slug.ToUpperInvariant());

        // Assert
        found.Should().NotBeNull();
        found!.Id.Should().Be(product.Id);
    }

    [Fact]
    public async Task SearchAsync_FilterByCategory_ReturnsOnlyMatchingProducts()
    {
        // Arrange
        await using var writeContext = _fixture.CreateContext();
        var category = new Category("Search Test Category", new Slug($"search-test-cat-{Guid.NewGuid():N}"));
        writeContext.Categories.Add(category);
        await writeContext.SaveChangesAsync();

        var writeRepository = new ProductRepository(writeContext);
        var matching = new Product(
            "Matching Product",
            new Sku(UniqueSku("MAT")),
            new Slug($"matching-product-{Guid.NewGuid():N}"),
            new Money(10m, "USD"),
            ProductType.Physical,
            categoryId: category.Id);
        var nonMatching = new Product(
            "Other Product",
            new Sku(UniqueSku("OTH")),
            new Slug($"other-product-{Guid.NewGuid():N}"),
            new Money(10m, "USD"),
            ProductType.Physical);

        await writeRepository.AddAsync(matching);
        await writeRepository.AddAsync(nonMatching);
        await writeContext.SaveChangesAsync();

        // Act
        await using var readContext = _fixture.CreateContext();
        var readRepository = new ProductRepository(readContext);
        var (items, totalCount) = await readRepository.SearchAsync(categoryId: category.Id, pageNumber: 1, pageSize: 10);

        // Assert
        items.Should().ContainSingle(p => p.Id == matching.Id);
        items.Should().NotContain(p => p.Id == nonMatching.Id);
        totalCount.Should().Be(1);
    }

    [Fact]
    public async Task Update_ChangedPrice_PersistsNewPrice()
    {
        // Arrange
        await using var writeContext = _fixture.CreateContext();
        var writeRepository = new ProductRepository(writeContext);
        var product = new Product(
            "Update Test Product",
            new Sku(UniqueSku("UPD")),
            new Slug($"update-test-product-{Guid.NewGuid():N}"),
            new Money(25m, "USD"),
            ProductType.Physical);
        await writeRepository.AddAsync(product);
        await writeContext.SaveChangesAsync();

        // Act — load fresh, mutate, persist via a new context+repository (mirrors handler flow)
        await using var updateContext = _fixture.CreateContext();
        var updateRepository = new ProductRepository(updateContext);
        var loaded = await updateRepository.GetByIdAsync(product.Id);
        loaded!.UpdatePrice(new Money(30m, "USD"));
        updateRepository.MarkPriceAsModified(loaded);
        await updateContext.SaveChangesAsync();

        await using var readContext = _fixture.CreateContext();
        var readRepository = new ProductRepository(readContext);
        var reloaded = await readRepository.GetByIdAsync(product.Id);

        // Assert
        reloaded!.Price.Amount.Should().Be(30m);
    }
}
