using MapsterMapper;
using MediatR;
using Vendix.Application.Common.Exceptions;
using Vendix.Application.Common.Models;
using Vendix.Application.Ordering.DTOs;
using Vendix.Domain.Ordering.Entities;
using Vendix.Domain.Ordering.Repositories;

namespace Vendix.Application.Ordering.Queries;

/// <summary>
/// Query to get a single order by its ID.
/// </summary>
public sealed record GetOrderByIdQuery(Guid Id) : IRequest<Result<OrderDto>>;

/// <summary>
/// Handler for <see cref="GetOrderByIdQuery"/>.
/// </summary>
public sealed class GetOrderByIdQueryHandler(
    IOrderRepository orderRepository,
    IMapper mapper) : IRequestHandler<GetOrderByIdQuery, Result<OrderDto>>
{
    /// <inheritdoc />
    public async Task<Result<OrderDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.Id, cancellationToken);
        if (order is null)
        {
            throw NotFoundException.ForEntity<Order>(request.Id);
        }

        return Result<OrderDto>.Success(mapper.Map<OrderDto>(order));
    }
}
