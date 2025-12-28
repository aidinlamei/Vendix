using Mapster;
using MediatR;
using Vendix.Application.Catalog.DTOs;
using Vendix.Application.Common.Exceptions;
using Vendix.Domain.Catalog.Repositories;

namespace Vendix.Application.Catalog.Queries;

public record GetBrandByIdQuery(Guid Id) : IRequest<BrandDto>;

public class GetBrandByIdQueryHandler(
    IBrandRepository brandRepository) : IRequestHandler<GetBrandByIdQuery, BrandDto>
{
    public async Task<BrandDto> Handle(GetBrandByIdQuery request, CancellationToken cancellationToken)
    {
        var brand = await brandRepository.GetByIdAsync(request.Id, cancellationToken);
        if (brand is null)
        {
            throw new NotFoundException("Brand", request.Id);
        }

        return brand.Adapt<BrandDto>();
    }
}

