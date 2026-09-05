using FluentValidation;
using MediatR;
using Vendix.Application.Common.Exceptions;
using Vendix.Application.Common.Interfaces;
using Vendix.Application.Common.Models;
using Vendix.Domain.Ordering.Entities;
using Vendix.Domain.Ordering.Enums;
using Vendix.Domain.Ordering.Repositories;

namespace Vendix.Application.Ordering.Commands;

/// <summary>
/// Command to update an order's status (admin action).
/// </summary>
public sealed record UpdateOrderStatusCommand(Guid OrderId, OrderStatus Status) : IRequest<Result>;

/// <summary>
/// Handler for <see cref="UpdateOrderStatusCommand"/>.
/// </summary>
public sealed class UpdateOrderStatusCommandHandler(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateOrderStatusCommand, Result>
{
    /// <inheritdoc />
    public async Task<Result> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            throw NotFoundException.ForEntity<Order>(request.OrderId);
        }

        try
        {
            order.UpdateStatus(request.Status);
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

/// <summary>
/// Validator for <see cref="UpdateOrderStatusCommand"/>.
/// </summary>
public sealed class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty().WithMessage("Order id is required.");
        RuleFor(x => x.Status).IsInEnum().WithMessage("Invalid order status.");
    }
}
