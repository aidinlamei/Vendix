using FluentValidation;
using MapsterMapper;
using MediatR;
using Vendix.Application.Basket.DTOs;
using Vendix.Application.Common.Interfaces;
using Vendix.Application.Common.Models;
using Vendix.Domain.Basket.Repositories;

namespace Vendix.Application.Basket.Commands;

/// <summary>
/// Command to remove a product from a buyer's basket entirely.
/// </summary>
public sealed record RemoveFromBasketCommand(string BuyerId, Guid ProductId) : IRequest<Result<BasketDto>>;

/// <summary>
/// Handler for <see cref="RemoveFromBasketCommand"/>.
/// </summary>
public sealed class RemoveFromBasketCommandHandler(
    IBasketRepository basketRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<RemoveFromBasketCommand, Result<BasketDto>>
{
    /// <inheritdoc />
    public async Task<Result<BasketDto>> Handle(RemoveFromBasketCommand request, CancellationToken cancellationToken)
    {
        var basket = await basketRepository.GetByBuyerIdAsync(request.BuyerId, cancellationToken);
        if (basket is null)
        {
            return Result<BasketDto>.Failure("Basket not found.");
        }

        basket.RemoveItem(request.ProductId);
        basketRepository.Update(basket);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<BasketDto>.Success(mapper.Map<BasketDto>(basket));
    }
}

/// <summary>
/// Validator for <see cref="RemoveFromBasketCommand"/>.
/// </summary>
public sealed class RemoveFromBasketCommandValidator : AbstractValidator<RemoveFromBasketCommand>
{
    public RemoveFromBasketCommandValidator()
    {
        RuleFor(x => x.BuyerId).NotEmpty().WithMessage("Buyer id is required.");
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("Product id is required.");
    }
}
