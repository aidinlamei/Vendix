using Vendix.Domain.Basket.Entities;
using Vendix.Domain.Catalog.ValueObjects;

namespace Vendix.Domain.Tests.Basket;

public class BasketTests
{
    private static Money DefaultPrice() => new(10m, "USD");

    [Fact]
    public void Constructor_ValidBuyerId_CreatesEmptyBasket()
    {
        var basket = new Vendix.Domain.Basket.Entities.Basket("buyer-1");

        basket.BuyerId.Should().Be("buyer-1");
        basket.Items.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Constructor_EmptyOrNullBuyerId_ThrowsArgumentException(string? buyerId)
    {
        var act = () => new Vendix.Domain.Basket.Entities.Basket(buyerId!);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddItem_NewProduct_AddsItemWithGivenQuantity()
    {
        var basket = new Vendix.Domain.Basket.Entities.Basket("buyer-1");
        var productId = Guid.NewGuid();

        basket.AddItem(productId, "Widget", "widget", "SKU-1", DefaultPrice(), 2, null);

        basket.Items.Should().ContainSingle();
        basket.Items.First().Quantity.Should().Be(2);
    }

    [Fact]
    public void AddItem_ExistingProduct_IncreasesQuantityInstesadOfDuplicating()
    {
        var basket = new Vendix.Domain.Basket.Entities.Basket("buyer-1");
        var productId = Guid.NewGuid();

        basket.AddItem(productId, "Widget", "widget", "SKU-1", DefaultPrice(), 2, null);
        basket.AddItem(productId, "Widget", "widget", "SKU-1", DefaultPrice(), 3, null);

        basket.Items.Should().ContainSingle();
        basket.Items.First().Quantity.Should().Be(5);
    }

    [Fact]
    public void AddItem_ZeroOrNegativeQuantity_ThrowsArgumentException()
    {
        var basket = new Vendix.Domain.Basket.Entities.Basket("buyer-1");

        var act = () => basket.AddItem(Guid.NewGuid(), "Widget", "widget", "SKU-1", DefaultPrice(), 0, null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SetItemQuantity_ZeroQuantity_RemovesItem()
    {
        var basket = new Vendix.Domain.Basket.Entities.Basket("buyer-1");
        var productId = Guid.NewGuid();
        basket.AddItem(productId, "Widget", "widget", "SKU-1", DefaultPrice(), 2, null);

        basket.SetItemQuantity(productId, 0);

        basket.Items.Should().BeEmpty();
    }

    [Fact]
    public void RemoveItem_ExistingProduct_RemovesIt()
    {
        var basket = new Vendix.Domain.Basket.Entities.Basket("buyer-1");
        var productId = Guid.NewGuid();
        basket.AddItem(productId, "Widget", "widget", "SKU-1", DefaultPrice(), 1, null);

        basket.RemoveItem(productId);

        basket.Items.Should().BeEmpty();
    }

    [Fact]
    public void Clear_WithItems_EmptiesBasket()
    {
        var basket = new Vendix.Domain.Basket.Entities.Basket("buyer-1");
        basket.AddItem(Guid.NewGuid(), "Widget", "widget", "SKU-1", DefaultPrice(), 1, null);
        basket.AddItem(Guid.NewGuid(), "Gadget", "gadget", "SKU-2", DefaultPrice(), 1, null);

        basket.Clear();

        basket.Items.Should().BeEmpty();
    }
}
