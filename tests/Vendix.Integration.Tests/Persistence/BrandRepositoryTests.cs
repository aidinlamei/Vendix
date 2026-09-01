using Microsoft.EntityFrameworkCore;
using Vendix.Domain.Catalog.Entities;
using Vendix.Domain.Catalog.ValueObjects;
using Vendix.Infrastructure.Persistence.Repositories;

namespace Vendix.Integration.Tests.Persistence;

/// <summary>
/// Integration tests for BrandRepository against a real PostgreSQL database.
/// Tests verify that the repository correctly persists and retrieves brands,
/// including slug uniqueness constraints and soft deletion filtering.
/// </summary>
[Collection(nameof(DatabaseCollection))]
public class BrandRepositoryTests
{
    private readonly DatabaseFixture _fixture;

    public BrandRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AddAsync_ThenGetBySlug_ReturnsPersistedBrand()
    {
        // Arrange
        await using var writeContext = _fixture.CreateContext();
        var writeRepository = new BrandRepository(writeContext);
        var slug = $"acme-{Guid.NewGuid():N}";
        var brand = new Brand("Acme", new Slug(slug), "https://example.com/logo.png");

        // Act
        await writeRepository.AddAsync(brand);
        await writeContext.SaveChangesAsync();

        await using var readContext = _fixture.CreateContext();
        var readRepository = new BrandRepository(readContext);
        var found = await readRepository.GetBySlugAsync(slug);

        // Assert
        found.Should().NotBeNull();
        found!.Name.Should().Be("Acme");
        found.LogoUrl.Should().Be("https://example.com/logo.png");
    }

    [Fact]
    public async Task AddAsync_DuplicateSlug_ThrowsOnSaveDueToUniqueIndex()
    {
        // Arrange
        await using var writeContext = _fixture.CreateContext();
        var writeRepository = new BrandRepository(writeContext);
        var slug = $"dup-brand-{Guid.NewGuid():N}";
        await writeRepository.AddAsync(new Brand("First", new Slug(slug)));
        await writeContext.SaveChangesAsync();

        // Act
        await using var secondContext = _fixture.CreateContext();
        var secondRepository = new BrandRepository(secondContext);
        await secondRepository.AddAsync(new Brand("Second", new Slug(slug)));
        var act = async () => await secondContext.SaveChangesAsync();

        // Assert — the unique index on Slug (BrandConfiguration) rejects the duplicate at the DB level
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task MarkAsDeleted_ThenGetAll_ExcludesSoftDeletedBrand()
    {
        // Arrange
        await using var writeContext = _fixture.CreateContext();
        var writeRepository = new BrandRepository(writeContext);
        var brand = new Brand("To Be Deleted", new Slug($"to-be-deleted-{Guid.NewGuid():N}"));
        await writeRepository.AddAsync(brand);
        await writeContext.SaveChangesAsync();

        // Act
        await using var deleteContext = _fixture.CreateContext();
        var deleteRepository = new BrandRepository(deleteContext);
        var loaded = await deleteContext.Brands.FirstAsync(b => b.Id == brand.Id);
        loaded.MarkAsDeleted("test-user");
        await deleteContext.SaveChangesAsync();

        await using var readContext = _fixture.CreateContext();
        var readRepository = new BrandRepository(readContext);
        var all = await readRepository.GetAllAsync();

        // Assert — BrandConfiguration's global query filter (!IsDeleted) hides it
        all.Should().NotContain(b => b.Id == brand.Id);
    }
}
