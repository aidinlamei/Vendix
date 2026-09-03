using MediatR;
using Vendix.Application.Common.Exceptions;
using Vendix.Application.Common.Interfaces;
using Vendix.Application.Common.Models;
using Vendix.Domain.Ordering.Entities;
using Vendix.Domain.Ordering.Repositories;

namespace Vendix.Application.Ordering.Commands;

/// <summary>
/// Command to cancel an order (buyer or admin action).
/// </summary>
public sealed record CancelOrderCommand(Guid OrderId) : IRequest<Result>;

/// <summary>
/// Handler for <see cref="CancelOrderCommand"/>.
/// </summary>
public sealed class CancelOrderCommandHandler(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CancelOrderCommand, Result>
{
    /// <inheritdoc />
    public async Task<Result> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            throw NotFoundException.ForEntity<Order>(request.OrderId);
        }

        try
        {
            order.Cancel();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }

        orderRepository.Update(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
