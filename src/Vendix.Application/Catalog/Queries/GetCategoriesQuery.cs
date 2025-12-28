using Mapster;
using MediatR;
using Vendix.Application.Catalog.DTOs;
using Vendix.Application.Common.Attributes;
using Vendix.Domain.Catalog.Repositories;

namespace Vendix.Application.Catalog.Queries;

[CacheableQuery(Key = "categories", ExpiryMinutes = 30)]
public record GetCategoriesQuery(bool IncludeChildren = false) : IRequest<List<CategoryListDto>>;

public class GetCategoriesQueryHandler(
    ICategoryRepository categoryRepository) : IRequestHandler<GetCategoriesQuery, List<CategoryListDto>>
{
    public async Task<List<CategoryListDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await categoryRepository.GetRootCategoriesAsync(cancellationToken);
        return categories.Adapt<List<CategoryListDto>>();
    }
}

