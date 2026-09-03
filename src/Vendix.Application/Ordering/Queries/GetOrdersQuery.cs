using MapsterMapper;
using MediatR;
using Vendix.Application.Common.Models;
using Vendix.Application.Ordering.DTOs;
using Vendix.Domain.Ordering.Enums;
using Vendix.Domain.Ordering.Repositories;

namespace Vendix.Application.Ordering.Queries;

/// <summary>
/// Query to search/paginate all orders across all buyers (admin index page).
/// </summary>
public sealed record GetOrdersQuery(
    int PageNumber = 1,
    int PageSize = 10,
    OrderStatus? Status = null) : IRequest<PaginatedList<OrderListDto>>;

/// <summary>
/// Handler for <see cref="GetOrdersQuery"/>.
/// </summary>
public sealed class GetOrdersQueryHandler(
    IOrderRepository orderRepository,
    IMapper mapper) : IRequestHandler<GetOrdersQuery, PaginatedList<OrderListDto>>
{
    /// <inheritdoc />
    public async Task<PaginatedList<OrderListDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var (orders, totalCount) = await orderRepository.SearchAsync(
            status: request.Status,
            pageNumber: request.PageNumber,
            pageSize: request.PageSize,
            cancellationToken: cancellationToken);

        var dtos = orders.Select(mapper.Map<OrderListDto>).ToList();

        return new PaginatedList<OrderListDto>(dtos, totalCount, request.PageNumber, request.PageSize);
    }
}
