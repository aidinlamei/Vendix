using MapsterMapper;
using MediatR;
using Vendix.Application.Catalog.DTOs;
using Vendix.Application.Common.Attributes;
using Vendix.Application.Common.Models;
using Vendix.Domain.Catalog.Repositories;

namespace Vendix.Application.Catalog.Queries;

/// <summary>
/// Query to get a paginated list of products with optional filters.
/// </summary>
/// <param name="PageNumber">The page number (1-based).</param>
/// <param name="PageSize">The page size.</param>
/// <param name="SearchTerm">Optional search term for name/description.</param>
/// <param name="CategoryId">Optional category filter.</param>
/// <param name="BrandId">Optional brand filter.</param>
/// <param name="MinPrice">Optional minimum price filter.</param>
/// <param name="MaxPrice">Optional maximum price filter.</param>
[CacheableQuery(Key = "products", ExpiryMinutes = 5)]
public sealed record GetProductsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    Guid? CategoryId = null,
    Guid? BrandId = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null) : IRequest<PaginatedList<ProductListDto>>;

/// <summary>
/// Handler for <see cref="GetProductsQuery"/>.
/// </summary>
public sealed class GetProductsQueryHandler(
    IProductRepository productRepository,
    IMapper mapper) : IRequestHandler<GetProductsQuery, PaginatedList<ProductListDto>>
{
    /// <inheritdoc />
    public async Task<PaginatedList<ProductListDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var (products, totalCount) = await productRepository.SearchAsync(
            request.SearchTerm,
            request.CategoryId,
            request.BrandId,
            request.MinPrice,
            request.MaxPrice,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var dtos = products.Select(p => mapper.Map<ProductListDto>(p)).ToList();

        return new PaginatedList<ProductListDto>(
            dtos,
            totalCount,
            request.PageNumber,
            request.PageSize);
    }
}
