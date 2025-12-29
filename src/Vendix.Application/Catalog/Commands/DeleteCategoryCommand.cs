using MediatR;
using Vendix.Application.Common.Exceptions;
using Vendix.Application.Common.Interfaces;
using Vendix.Application.Common.Models;
using Vendix.Domain.Catalog.Repositories;

namespace Vendix.Application.Catalog.Commands;

public record DeleteCategoryCommand(Guid Id) : IRequest<Result>;

public class DeleteCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    ICacheService cacheService) : IRequestHandler<DeleteCategoryCommand, Result>
{
    public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetWithChildrenAsync(request.Id, cancellationToken);
        if (category is null)
        {
            throw new NotFoundException("Category", request.Id);
        }

        // Check if category has children
        if (category.SubCategories.Any(c => !c.IsDeleted))
        {
            throw new BusinessRuleException("HasChildren", "Cannot delete category with active subcategories");
        }

        // Soft delete
        category.MarkAsDeleted(currentUserService.UserId);
        
        categoryRepository.Update(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        await cacheService.RemoveByPrefixAsync("categories", cancellationToken);

        return Result.Success();
    }
}

