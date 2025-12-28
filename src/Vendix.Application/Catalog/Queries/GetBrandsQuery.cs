using Mapster;
using MediatR;
using Vendix.Application.Catalog.DTOs;
using Vendix.Application.Common.Attributes;
using Vendix.Domain.Catalog.Repositories;

namespace Vendix.Application.Catalog.Queries;

[CacheableQuery(Key = "brands", ExpiryMinutes = 30)]
public record GetBrandsQuery : IRequest<List<BrandListDto>>;

public class GetBrandsQueryHandler(
    IBrandRepository brandRepository) : IRequestHandler<GetBrandsQuery, List<BrandListDto>>
{
    public async Task<List<BrandListDto>> Handle(GetBrandsQuery request, CancellationToken cancellationToken)
    {
        var brands = await brandRepository.GetAllAsync(cancellationToken);
        return brands.Adapt<List<BrandListDto>>();
    }
}

