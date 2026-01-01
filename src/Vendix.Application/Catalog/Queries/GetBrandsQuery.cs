using Mapster;
using MediatR;
using Vendix.Application.Catalog.DTOs;
using Vendix.Application.Common.Attributes;
using Vendix.Domain.Catalog.Repositories;

namespace Vendix.Application.Catalog.Queries;

[CacheableQuery(Key = "brands", ExpiryMinutes = 30)]
public record GetBrandsQuery : IRequest<List<BrandListDto>>;

public class GetBrandsQueryHandler(
    IBrandRepository brandRepository,
    IProductRepository productRepository) : IRequestHandler<GetBrandsQuery, List<BrandListDto>>
{
    public async Task<List<BrandListDto>> Handle(GetBrandsQuery request, CancellationToken cancellationToken)
    {
        var brands = await brandRepository.GetAllAsync(cancellationToken);
        var brandDtos = new List<BrandListDto>();

        foreach (var brand in brands)
        {
            var productCount = await productRepository.CountByBrandAsync(brand.Id, cancellationToken);
            var dto = brand.Adapt<BrandListDto>();
            brandDtos.Add(dto with { ProductCount = productCount });
        }

        return brandDtos;
    }
}

