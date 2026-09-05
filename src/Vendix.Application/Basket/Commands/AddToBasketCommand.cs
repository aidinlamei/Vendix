using FluentValidation;
using MapsterMapper;
using MediatR;
using Vendix.Application.Basket.DTOs;
using Vendix.Application.Common.Interfaces;
using Vendix.Application.Common.Models;
using Vendix.Domain.Basket.Repositories;
using Vendix.Domain.Catalog.Repositories;

namespace Vendix.Application.Basket.Commands;

/// <summary>
/// Command to add a product to a buyer's basket, or increase its quantity if already present.
/// </summary>
/// <remarks>
/// Product name/slug/SKU/price/image are always re-read from the authoritative
/// <see cref="Domain.Catalog.Entities.Product"/> on the server — the client never supplies
/// pricing, so a tampered client request can't put an arbitrary price into the basket.
/// </remarks>
public sealed record AddToBasketCommand(string BuyerId, Guid ProductId, int Quantity = 1) : IRequest<Result<BasketDto>>;

/// <summary>
/// Handler for <see cref="AddToBasketCommand"/>.
/// </summary>
public sealed class AddToBasketCommandHandler(
    IBasketRepository basketRepository,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<AddToBasketCommand, Result<BasketDto>>
{
    /// <inheritdoc />
    public async Task<Result<BasketDto>> Handle(AddToBasketCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
        {
            return Result<BasketDto>.Failure("Product not found.");
        }

        var imageUrl = product.Images.FirstOrDefault(i => i.IsMain)?.Url
            ?? product.Images.FirstOrDefault()?.Url;

        var basket = await basketRepository.GetByBuyerIdAsync(request.BuyerId, cancellationToken);
        if (basket is null)
        {
            basket = new Domain.Basket.Entities.Basket(request.BuyerId);
            basket.AddItem(product.Id, product.Name, product.Slug.Value, product.Sku.Value, product.Price, request.Quantity, imageUrl);
            await basketRepository.AddAsync(basket, cancellationToken);
        }
        else
        {
            basket.AddItem(product.Id, product.Name, product.Slug.Value, product.Sku.Value, product.Price, request.Quantity, imageUrl);
            basketRepository.Update(basket);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<BasketDto>.Success(mapper.Map<BasketDto>(basket));
    }
}

/// <summary>
/// Validator for <see cref="AddToBasketCommand"/>.
/// </summary>
public sealed class AddToBasketCommandValidator : AbstractValidator<AddToBasketCommand>
{
    public AddToBasketCommandValidator()
    {
        RuleFor(x => x.BuyerId).NotEmpty().WithMessage("Buyer id is required.");
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("Product id is required.");
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than zero.");
    }
}
