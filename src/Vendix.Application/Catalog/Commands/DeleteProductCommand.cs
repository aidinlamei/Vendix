using MediatR;
using Vendix.Application.Common.Exceptions;
using Vendix.Application.Common.Interfaces;
using Vendix.Application.Common.Models;
using Vendix.Domain.Catalog.Repositories;

namespace Vendix.Application.Catalog.Commands;

/// <summary>
/// Command to delete (soft delete) a product.
/// </summary>
/// <param name="Id">The product ID to delete.</param>
public sealed record DeleteProductCommand(Guid Id) : IRequest<Result>;

/// <summary>
/// Handler for <see cref="DeleteProductCommand"/>.
/// </summary>
public sealed class DeleteProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    ICacheService cacheService) : IRequestHandler<DeleteProductCommand, Result>
{
    /// <inheritdoc />
    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null)
        {
            throw NotFoundException.ForEntity<Domain.Catalog.Entities.Product>(request.Id);
        }

        // Soft delete the product
        product.MarkAsDeleted();

        productRepository.Update(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        await cacheService.RemoveByPrefixAsync("products", cancellationToken);

        return Result.Success();
    }
}
