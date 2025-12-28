using Mapster;
using MediatR;
using Vendix.Application.Catalog.DTOs;
using Vendix.Application.Common.Exceptions;
using Vendix.Domain.Catalog.Repositories;

namespace Vendix.Application.Catalog.Queries;

public record GetBrandBySlugQuery(string Slug) : IRequest<BrandDto>;

public class GetBrandBySlugQueryHandler(
    IBrandRepository brandRepository) : IRequestHandler<GetBrandBySlugQuery, BrandDto>
{
    public async Task<BrandDto> Handle(GetBrandBySlugQuery request, CancellationToken cancellationToken)
    {
        var brand = await brandRepository.GetBySlugAsync(request.Slug, cancellationToken);
        if (brand is null)
        {
            throw new NotFoundException("Brand", request.Slug);
        }

        return brand.Adapt<BrandDto>();
    }
}

