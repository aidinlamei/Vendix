using MapsterMapper;
using MediatR;
using Vendix.Application.Basket.DTOs;
using Vendix.Domain.Basket.Repositories;

namespace Vendix.Application.Basket.Queries;

/// <summary>
/// Query to get a buyer's basket. Returns an empty basket DTO (not an error) if the buyer
/// doesn't have one yet — a fresh visitor with no basket is a normal state, not a failure.
/// </summary>
public sealed record GetBasketQuery(string BuyerId) : IRequest<BasketDto>;

/// <summary>
/// Handler for <see cref="GetBasketQuery"/>.
/// </summary>
public sealed class GetBasketQueryHandler(
    IBasketRepository basketRepository,
    IMapper mapper) : IRequestHandler<GetBasketQuery, BasketDto>
{
    /// <inheritdoc />
    public async Task<BasketDto> Handle(GetBasketQuery request, CancellationToken cancellationToken)
    {
        var basket = await basketRepository.GetByBuyerIdAsync(request.BuyerId, cancellationToken);
        return basket is null
            ? new BasketDto { BuyerId = request.BuyerId }
            : mapper.Map<BasketDto>(basket);
    }
}
