using MapsterMapper;
using MediatR;
using Vendix.Application.Catalog.DTOs;
using Vendix.Application.Common.Exceptions;
using Vendix.Application.Common.Models;
using Vendix.Domain.Catalog.Entities;
using Vendix.Domain.Catalog.Repositories;

namespace Vendix.Application.Catalog.Queries;

/// <summary>
/// Query to get a product by its ID.
/// </summary>
/// <param name="Id">The product ID.</param>
public sealed record GetProductByIdQuery(Guid Id) : IRequest<Result<ProductDto>>;

/// <summary>
/// Handler for <see cref="GetProductByIdQuery"/>.
/// </summary>
public sealed class GetProductByIdQueryHandler(
    IProductRepository productRepository,
    IMapper mapper) : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
{
    /// <inheritdoc />
    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null)
        {
            throw NotFoundException.ForEntity<Product>(request.Id);
        }

        var dto = mapper.Map<ProductDto>(product);
        return Result<ProductDto>.Success(dto);
    }
}
