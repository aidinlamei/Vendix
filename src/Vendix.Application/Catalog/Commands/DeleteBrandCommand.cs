using MediatR;
using Vendix.Application.Common.Exceptions;
using Vendix.Application.Common.Interfaces;
using Vendix.Application.Common.Models;
using Vendix.Domain.Catalog.Repositories;

namespace Vendix.Application.Catalog.Commands;

public record DeleteBrandCommand(Guid Id) : IRequest<Result>;

public class DeleteBrandCommandHandler(
    IBrandRepository brandRepository,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService) : IRequestHandler<DeleteBrandCommand, Result>
{
    public async Task<Result> Handle(DeleteBrandCommand request, CancellationToken cancellationToken)
    {
        var brand = await brandRepository.GetByIdAsync(request.Id, cancellationToken);
        if (brand is null)
        {
            throw new NotFoundException("Brand", request.Id);
        }

        // Check if brand has products
        var products = await productRepository.GetByBrandAsync(request.Id, cancellationToken);
        if (products.Any())
        {
            throw new BusinessRuleException("HasProducts", "Cannot delete brand with associated products");
        }

        // Soft delete
        brand.MarkAsDeleted(currentUserService.UserId);
        
        brandRepository.Update(brand);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

