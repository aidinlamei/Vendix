using Vendix.Application.Ordering.Commands;

namespace Vendix.Application.Tests.Ordering;

public class PlaceOrderCommandValidatorTests
{
    private readonly PlaceOrderCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_Passes()
    {
        var command = new PlaceOrderCommand("buyer-1", "buyer@example.com", "123 Main St", 5m);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Validate_InvalidEmail_Fails(string email)
    {
        var command = new PlaceOrderCommand("buyer-1", email, "123 Main St", 0m);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_EmptyShippingAddress_Fails()
    {
        var command = new PlaceOrderCommand("buyer-1", "buyer@example.com", "", 0m);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_NegativeShippingCost_Fails()
    {
        var command = new PlaceOrderCommand("buyer-1", "buyer@example.com", "123 Main St", -1m);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }
}
