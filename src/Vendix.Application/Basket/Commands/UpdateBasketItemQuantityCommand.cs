using FluentValidation;
using MapsterMapper;
using MediatR;
using Vendix.Application.Basket.DTOs;
using Vendix.Application.Common.Interfaces;
using Vendix.Application.Common.Models;
using Vendix.Domain.Basket.Repositories;

namespace Vendix.Application.Basket.Commands;

/// <summary>
/// Command to set the exact quantity of a basket item. A quantity of zero removes the item.
/// </summary>
public sealed record UpdateBasketItemQuantityCommand(string BuyerId, Guid ProductId, int Quantity) : IRequest<Result<BasketDto>>;

/// <summary>
/// Handler for <see cref="UpdateBasketItemQuantityCommand"/>.
/// </summary>
public sealed class UpdateBasketItemQuantityCommandHandler(
    IBasketRepository basketRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<UpdateBasketItemQuantityCommand, Result<BasketDto>>
{
    /// <inheritdoc />
    public async Task<Result<BasketDto>> Handle(UpdateBasketItemQuantityCommand request, CancellationToken cancellationToken)
    {
        var basket = await basketRepository.GetByBuyerIdAsync(request.BuyerId, cancellationToken);
        if (basket is null)
        {
            return Result<BasketDto>.Failure("Basket not found.");
        }

        basket.SetItemQuantity(request.ProductId, request.Quantity);
        basketRepository.Update(basket);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<BasketDto>.Success(mapper.Map<BasketDto>(basket));
    }
}

/// <summary>
/// Validator for <see cref="UpdateBasketItemQuantityCommand"/>.
/// </summary>
public sealed class UpdateBasketItemQuantityCommandValidator : AbstractValidator<UpdateBasketItemQuantityCommand>
{
    public UpdateBasketItemQuantityCommandValidator()
    {
        RuleFor(x => x.BuyerId).NotEmpty().WithMessage("Buyer id is required.");
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("Product id is required.");
        RuleFor(x => x.Quantity).GreaterThanOrEqualTo(0).WithMessage("Quantity cannot be negative.");
    }
}
