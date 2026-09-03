using Vendix.Domain.Ordering.Entities;
using Vendix.Domain.Ordering.Enums;

namespace Vendix.Domain.Tests.Ordering;

public class OrderTests
{
    private static Order CreateValidOrder() =>
        new("buyer-1", "buyer@example.com", "123 Main St", "USD", 5m);

    [Fact]
    public void Constructor_ValidInputs_CreatesPendingOrderWithGeneratedNumber()
    {
        var order = CreateValidOrder();

        order.Status.Should().Be(OrderStatus.Pending);
        order.OrderNumber.Should().NotBeNull();
        order.Currency.Should().Be("USD");
        order.ShippingCost.Should().Be(5m);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Constructor_EmptyBuyerEmail_ThrowsArgumentException(string? email)
    {
        var act = () => new Order("buyer-1", email!, "123 Main St", "USD", 0m);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_NegativeShippingCost_ThrowsArgumentException()
    {
        var act = () => new Order("buyer-1", "buyer@example.com", "123 Main St", "USD", -1m);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddItem_ValidItem_IncludedInSubtotalAndTotal()
    {
        var order = CreateValidOrder();

        order.AddItem(Guid.NewGuid(), "Widget", "SKU-1", 10m, 2, null);

        order.Subtotal.Should().Be(20m);
        order.Total.Should().Be(25m); // 20 subtotal + 5 shipping
    }

    [Fact]
    public void Cancel_PendingOrder_SetsCancelledStatus()
    {
        var order = CreateValidOrder();

        order.Cancel();

        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void Cancel_AlreadyCancelled_ThrowsInvalidOperationException()
    {
        var order = CreateValidOrder();
        order.Cancel();

        var act = order.Cancel;

        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(OrderStatus.Shipped)]
    [InlineData(OrderStatus.Delivered)]
    public void Cancel_ShippedOrDelivered_ThrowsInvalidOperationException(OrderStatus status)
    {
        var order = CreateValidOrder();
        order.UpdateStatus(status);

        var act = order.Cancel;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void UpdateStatus_CancelledOrder_ThrowsInvalidOperationException()
    {
        var order = CreateValidOrder();
        order.Cancel();

        var act = () => order.UpdateStatus(OrderStatus.Processing);

        act.Should().Throw<InvalidOperationException>();
    }
}
