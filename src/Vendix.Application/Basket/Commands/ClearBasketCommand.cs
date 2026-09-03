using FluentValidation;
using MediatR;
using Vendix.Application.Common.Interfaces;
using Vendix.Application.Common.Models;
using Vendix.Domain.Basket.Repositories;

namespace Vendix.Application.Basket.Commands;

/// <summary>
/// Command to empty a buyer's basket.
/// </summary>
public sealed record ClearBasketCommand(string BuyerId) : IRequest<Result>;

/// <summary>
/// Handler for <see cref="ClearBasketCommand"/>.
/// </summary>
public sealed class ClearBasketCommandHandler(
    IBasketRepository basketRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<ClearBasketCommand, Result>
{
    /// <inheritdoc />
    public async Task<Result> Handle(ClearBasketCommand request, CancellationToken cancellationToken)
    {
        var basket = await basketRepository.GetByBuyerIdAsync(request.BuyerId, cancellationToken);
        if (basket is null)
        {
            return Result.Success();
        }

        basket.Clear();
        basketRepository.Update(basket);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

/// <summary>
/// Validator for <see cref="ClearBasketCommand"/>.
/// </summary>
public sealed class ClearBasketCommandValidator : AbstractValidator<ClearBasketCommand>
{
    public ClearBasketCommandValidator()
    {
        RuleFor(x => x.BuyerId).NotEmpty().WithMessage("Buyer id is required.");
    }
}
