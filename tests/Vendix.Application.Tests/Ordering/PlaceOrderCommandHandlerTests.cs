using Vendix.Application.Common.Interfaces;
using Vendix.Application.Ordering.Commands;
using Vendix.Domain.Basket.Repositories;
using Vendix.Domain.Catalog.ValueObjects;
using Vendix.Domain.Ordering.Entities;
using Vendix.Domain.Ordering.Repositories;

namespace Vendix.Application.Tests.Ordering;

public class PlaceOrderCommandHandlerTests
{
    private readonly IBasketRepository _basketRepository = Substitute.For<IBasketRepository>();
    private readonly IOrderRepository _orderRepository = Substitute.For<IOrderRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private PlaceOrderCommandHandler CreateHandler() =>
        new(_basketRepository, _orderRepository, _unitOfWork);

    [Fact]
    public async Task Handle_NoBasketForBuyer_ReturnsFailure()
    {
        _basketRepository.GetByBuyerIdAsync("buyer-1", Arg.Any<CancellationToken>())
            .Returns((Vendix.Domain.Basket.Entities.Basket?)null);

        var result = await CreateHandler().Handle(
            new PlaceOrderCommand("buyer-1", "buyer@example.com", "123 Main St"),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_EmptyBasket_ReturnsFailure()
    {
        var basket = new Vendix.Domain.Basket.Entities.Basket("buyer-1");
        _basketRepository.GetByBuyerIdAsync("buyer-1", Arg.Any<CancellationToken>()).Returns(basket);

        var result = await CreateHandler().Handle(
            new PlaceOrderCommand("buyer-1", "buyer@example.com", "123 Main St"),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_BasketWithItems_CreatesOrderAndClearsBasket()
    {
        var basket = new Vendix.Domain.Basket.Entities.Basket("buyer-1");
        basket.AddItem(Guid.NewGuid(), "Widget", "widget", "SKU-1", new Money(15m, "USD"), 2, null);
        _basketRepository.GetByBuyerIdAsync("buyer-1", Arg.Any<CancellationToken>()).Returns(basket);

        Order? capturedOrder = null;
        await _orderRepository.AddAsync(
            Arg.Do<Order>(o => capturedOrder = o),
            Arg.Any<CancellationToken>());

        var result = await CreateHandler().Handle(
            new PlaceOrderCommand("buyer-1", "buyer@example.com", "123 Main St", 5m),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Total.Should().Be(35m); // (15 * 2) + 5 shipping
        basket.Items.Should().BeEmpty(); // cleared after placing the order
        capturedOrder.Should().NotBeNull();
        capturedOrder!.Items.Should().ContainSingle(i => i.ProductName == "Widget" && i.Quantity == 2);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
