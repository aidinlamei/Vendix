using MapsterMapper;
using MediatR;
using Vendix.Application.Ordering.DTOs;
using Vendix.Domain.Ordering.Repositories;

namespace Vendix.Application.Ordering.Queries;

/// <summary>
/// Query to get all orders placed by a specific buyer, most recent first.
/// </summary>
public sealed record GetMyOrdersQuery(string BuyerId) : IRequest<List<OrderListDto>>;

/// <summary>
/// Handler for <see cref="GetMyOrdersQuery"/>.
/// </summary>
public sealed class GetMyOrdersQueryHandler(
    IOrderRepository orderRepository,
    IMapper mapper) : IRequestHandler<GetMyOrdersQuery, List<OrderListDto>>
{
    /// <inheritdoc />
    public async Task<List<OrderListDto>> Handle(GetMyOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await orderRepository.GetByBuyerIdAsync(request.BuyerId, cancellationToken);
        return orders.Select(mapper.Map<OrderListDto>).ToList();
    }
}
