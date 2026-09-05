using FluentValidation;
using MediatR;
using Vendix.Application.Common.Interfaces;
using Vendix.Application.Common.Models;
using Vendix.Application.Ordering.DTOs;
using Vendix.Domain.Basket.Repositories;
using Vendix.Domain.Ordering.Entities;
using Vendix.Domain.Ordering.Repositories;

namespace Vendix.Application.Ordering.Commands;

/// <summary>
/// Command to place an order from the buyer's current basket. The basket is emptied
/// (not deleted) on success so the buyer can keep shopping with the same basket row.
/// </summary>
public sealed record PlaceOrderCommand(
    string BuyerId,
    string BuyerEmail,
    string ShippingAddress,
    decimal ShippingCost = 0m) : IRequest<Result<PlaceOrderResultDto>>;

/// <summary>
/// Handler for <see cref="PlaceOrderCommand"/>.
/// </summary>
public sealed class PlaceOrderCommandHandler(
    IBasketRepository basketRepository,
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<PlaceOrderCommand, Result<PlaceOrderResultDto>>
{
    /// <inheritdoc />
    public async Task<Result<PlaceOrderResultDto>> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        var basket = await basketRepository.GetByBuyerIdAsync(request.BuyerId, cancellationToken);
        if (basket is null || basket.Items.Count == 0)
        {
            return Result<PlaceOrderResultDto>.Failure("Your basket is empty.");
        }

        var currency = basket.Items.First().UnitPrice.Currency;

        var order = new Order(request.BuyerId, request.BuyerEmail, request.ShippingAddress, currency, request.ShippingCost);
        foreach (var item in basket.Items)
        {
            order.AddItem(item.ProductId, item.ProductName, item.Sku, item.UnitPrice.Amount, item.Quantity, item.ImageUrl);
        }

        await orderRepository.AddAsync(order, cancellationToken);

        basket.Clear();
        basketRepository.Update(basket);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PlaceOrderResultDto>.Success(
            new PlaceOrderResultDto(order.Id, order.OrderNumber.Value, order.Total, order.Currency));
    }
}

/// <summary>
/// Validator for <see cref="PlaceOrderCommand"/>.
/// </summary>
public sealed class PlaceOrderCommandValidator : AbstractValidator<PlaceOrderCommand>
{
    public PlaceOrderCommandValidator()
    {
        RuleFor(x => x.BuyerId).NotEmpty().WithMessage("Buyer id is required.");

        RuleFor(x => x.BuyerEmail)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.ShippingAddress)
            .NotEmpty().WithMessage("Shipping address is required.")
            .MaximumLength(500).WithMessage("Shipping address must not exceed 500 characters.");

        RuleFor(x => x.ShippingCost)
            .GreaterThanOrEqualTo(0).WithMessage("Shipping cost cannot be negative.");
    }
}
