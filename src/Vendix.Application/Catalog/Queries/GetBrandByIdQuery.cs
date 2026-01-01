using Mapster;
using MediatR;
using Vendix.Application.Catalog.DTOs;
using Vendix.Application.Common.Exceptions;
using Vendix.Domain.Catalog.Repositories;

namespace Vendix.Application.Catalog.Queries;

public record GetBrandByIdQuery(Guid Id) : IRequest<BrandDto>;

public class GetBrandByIdQueryHandler(
    IBrandRepository brandRepository,
    IProductRepository productRepository) : IRequestHandler<GetBrandByIdQuery, BrandDto>
{
    public async Task<BrandDto> Handle(GetBrandByIdQuery request, CancellationToken cancellationToken)
    {
        var brand = await brandRepository.GetByIdAsync(request.Id, cancellationToken);
        if (brand is null)
        {
            throw new NotFoundException("Brand", request.Id);
        }

        var productCount = await productRepository.CountByBrandAsync(brand.Id, cancellationToken);
        var dto = brand.Adapt<BrandDto>();
        return dto with { ProductCount = productCount };
    }
}

