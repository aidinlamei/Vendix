using Vendix.Domain.Catalog.ValueObjects;
using Vendix.Infrastructure.Persistence.Repositories;

namespace Vendix.Integration.Tests.Persistence;

[Collection(nameof(DatabaseCollection))]
public class BasketRepositoryTests
{
    private readonly DatabaseFixture _fixture;

    public BasketRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AddAsync_ThenGetByBuyerId_ReturnsBasketWithItems()
    {
        var buyerId = $"buyer-{Guid.NewGuid():N}";

        await using var writeContext = _fixture.CreateContext();
        var writeRepository = new BasketRepository(writeContext);
        var basket = new Vendix.Domain.Basket.Entities.Basket(buyerId);
        basket.AddItem(Guid.NewGuid(), "Widget", "widget", "SKU-1", new Money(9.99m, "USD"), 3, null);

        await writeRepository.AddAsync(basket);
        await writeContext.SaveChangesAsync();

        await using var readContext = _fixture.CreateContext();
        var readRepository = new BasketRepository(readContext);
        var found = await readRepository.GetByBuyerIdAsync(buyerId);

        found.Should().NotBeNull();
        found!.Items.Should().ContainSingle(i => i.Quantity == 3 && i.UnitPrice.Amount == 9.99m);
    }

    [Fact]
    public async Task Update_AfterClear_PersistsEmptyBasket()
    {
        var buyerId = $"buyer-{Guid.NewGuid():N}";

        await using var writeContext = _fixture.CreateContext();
        var writeRepository = new BasketRepository(writeContext);
        var basket = new Vendix.Domain.Basket.Entities.Basket(buyerId);
        basket.AddItem(Guid.NewGuid(), "Widget", "widget", "SKU-1", new Money(5m, "USD"), 1, null);
        await writeRepository.AddAsync(basket);
        await writeContext.SaveChangesAsync();

        await using var clearContext = _fixture.CreateContext();
        var clearRepository = new BasketRepository(clearContext);
        var loaded = await clearRepository.GetByBuyerIdAsync(buyerId);
        loaded!.Clear();
        clearRepository.Update(loaded);
        await clearContext.SaveChangesAsync();

        await using var readContext = _fixture.CreateContext();
        var readRepository = new BasketRepository(readContext);
        var found = await readRepository.GetByBuyerIdAsync(buyerId);

        found!.Items.Should().BeEmpty();
    }
}
