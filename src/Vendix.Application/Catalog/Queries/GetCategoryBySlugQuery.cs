using Mapster;
using MediatR;
using Vendix.Application.Catalog.DTOs;
using Vendix.Application.Common.Exceptions;
using Vendix.Domain.Catalog.Repositories;
using Vendix.Domain.Catalog.ValueObjects;

namespace Vendix.Application.Catalog.Queries;

public record GetCategoryBySlugQuery(string Slug) : IRequest<CategoryDto>;

public class GetCategoryBySlugQueryHandler(
    ICategoryRepository categoryRepository) : IRequestHandler<GetCategoryBySlugQuery, CategoryDto>
{
    public async Task<CategoryDto> Handle(GetCategoryBySlugQuery request, CancellationToken cancellationToken)
    {
        var slug = new Slug(request.Slug);
        var category = await categoryRepository.GetBySlugAsync(slug.Value, cancellationToken);
        if (category is null)
        {
            throw new NotFoundException("Category", request.Slug);
        }

        return category.Adapt<CategoryDto>();
    }
}

