using Vendix.Domain.Ordering.ValueObjects;

namespace Vendix.Domain.Tests.Ordering;

public class OrderNumberTests
{
    [Fact]
    public void Generate_ReturnsValueMatchingPattern()
    {
        var orderNumber = OrderNumber.Generate();

        orderNumber.Value.Should().MatchRegex(OrderNumber.Pattern);
    }

    [Fact]
    public void Generate_CalledTwice_ReturnsDifferentValues()
    {
        var first = OrderNumber.Generate();
        var second = OrderNumber.Generate();

        first.Value.Should().NotBe(second.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-order-number")]
    [InlineData("ORD-2026-ABCDEF")]
    public void Constructor_InvalidFormat_ThrowsArgumentException(string value)
    {
        var act = () => new OrderNumber(value);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_ValidFormat_RoundTripsValue()
    {
        var generated = OrderNumber.Generate();

        var reconstructed = new OrderNumber(generated.Value);

        reconstructed.Should().Be(generated);
    }
}
