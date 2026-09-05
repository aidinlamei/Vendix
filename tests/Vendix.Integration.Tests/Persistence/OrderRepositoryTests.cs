using Vendix.Domain.Ordering.Entities;
using Vendix.Domain.Ordering.Enums;
using Vendix.Infrastructure.Persistence.Repositories;

namespace Vendix.Integration.Tests.Persistence;

[Collection(nameof(DatabaseCollection))]
public class OrderRepositoryTests
{
    private readonly DatabaseFixture _fixture;

    public OrderRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AddAsync_ThenGetById_ReturnsOrderWithItemsAndGeneratedNumber()
    {
        await using var writeContext = _fixture.CreateContext();
        var writeRepository = new OrderRepository(writeContext);
        var order = new Order($"buyer-{Guid.NewGuid():N}", "buyer@example.com", "123 Main St", "USD", 5m);
        order.AddItem(Guid.NewGuid(), "Widget", "SKU-1", 10m, 2, null);

        await writeRepository.AddAsync(order);
        await writeContext.SaveChangesAsync();

        await using var readContext = _fixture.CreateContext();
        var readRepository = new OrderRepository(readContext);
        var found = await readRepository.GetByIdAsync(order.Id);

        found.Should().NotBeNull();
        found!.OrderNumber.Value.Should().Be(order.OrderNumber.Value);
        found.Items.Should().ContainSingle(i => i.Quantity == 2);
        found.Total.Should().Be(25m);
    }

    [Fact]
    public async Task SearchAsync_FilterByStatus_ReturnsOnlyMatchingOrders()
    {
        var buyerId = $"buyer-{Guid.NewGuid():N}";

        await using var writeContext = _fixture.CreateContext();
        var writeRepository = new OrderRepository(writeContext);

        var pending = new Order(buyerId, "buyer@example.com", "123 Main St", "USD", 0m);
        pending.AddItem(Guid.NewGuid(), "Widget", "SKU-1", 10m, 1, null);
        await writeRepository.AddAsync(pending);

        var cancelled = new Order(buyerId, "buyer@example.com", "123 Main St", "USD", 0m);
        cancelled.AddItem(Guid.NewGuid(), "Gadget", "SKU-2", 10m, 1, null);
        cancelled.Cancel();
        await writeRepository.AddAsync(cancelled);

        await writeContext.SaveChangesAsync();

        await using var readContext = _fixture.CreateContext();
        var readRepository = new OrderRepository(readContext);
        var (items, totalCount) = await readRepository.SearchAsync(buyerId: buyerId, status: OrderStatus.Cancelled);

        items.Should().ContainSingle(o => o.Id == cancelled.Id);
        items.Should().NotContain(o => o.Id == pending.Id);
        totalCount.Should().Be(1);
    }
}
