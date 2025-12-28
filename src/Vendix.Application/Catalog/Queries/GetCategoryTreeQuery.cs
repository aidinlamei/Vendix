using MediatR;
using Vendix.Application.Catalog.DTOs;
using Vendix.Application.Common.Attributes;
using Vendix.Domain.Catalog.Entities;
using Vendix.Domain.Catalog.Repositories;

namespace Vendix.Application.Catalog.Queries;

[CacheableQuery(Key = "categories:tree", ExpiryMinutes = 30)]
public record GetCategoryTreeQuery : IRequest<List<CategoryTreeDto>>;

public class GetCategoryTreeQueryHandler(
    ICategoryRepository categoryRepository) : IRequestHandler<GetCategoryTreeQuery, List<CategoryTreeDto>>
{
    public async Task<List<CategoryTreeDto>> Handle(GetCategoryTreeQuery request, CancellationToken cancellationToken)
    {
        var rootCategories = await categoryRepository.GetRootCategoriesAsync(cancellationToken);
        return rootCategories.Select(c => MapToTree(c, 0)).ToList();
    }

    private static CategoryTreeDto MapToTree(Category category, int level)
    {
        return new CategoryTreeDto
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug.Value,
            Level = level,
            Children = category.SubCategories
                .Where(c => !c.IsDeleted)
                .Select(c => MapToTree(c, level + 1))
                .ToList()
        };
    }
}

