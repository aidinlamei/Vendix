using FluentValidation;
using MapsterMapper;
using MediatR;
using Vendix.Application.Catalog.DTOs;
using Vendix.Application.Common.Exceptions;
using Vendix.Application.Common.Models;
using Vendix.Domain.Catalog.Entities;
using Vendix.Domain.Catalog.Repositories;
using Vendix.Domain.Catalog.ValueObjects;

namespace Vendix.Application.Catalog.Queries;

/// <summary>
/// Query to get a product by its slug.
/// </summary>
/// <param name="Slug">The product slug.</param>
public sealed record GetProductBySlugQuery(string Slug) : IRequest<Result<ProductDto>>;

/// <summary>
/// Handler for <see cref="GetProductBySlugQuery"/>.
/// </summary>
public sealed class GetProductBySlugQueryHandler(
    IProductRepository productRepository,
    IMapper mapper) : IRequestHandler<GetProductBySlugQuery, Result<ProductDto>>
{
    /// <inheritdoc />
    public async Task<Result<ProductDto>> Handle(GetProductBySlugQuery request, CancellationToken cancellationToken)
    {
        var slug = new Slug(request.Slug);
        var product = await productRepository.GetBySlugAsync(slug, cancellationToken);
        if (product is null)
        {
            throw new NotFoundException("Product", request.Slug);
        }

        var dto = mapper.Map<ProductDto>(product);
        return Result<ProductDto>.Success(dto);
    }
}

/// <summary>
/// Validator for <see cref="GetProductBySlugQuery"/>.
/// </summary>
public sealed class GetProductBySlugQueryValidator : AbstractValidator<GetProductBySlugQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetProductBySlugQueryValidator"/> class.
    /// </summary>
    public GetProductBySlugQueryValidator()
    {
        RuleFor(x => x.Slug)
            .NotEmpty()
            .WithMessage("Slug is required.")
            .MinimumLength(Slug.MinLength)
            .WithMessage($"Slug must be at least {Slug.MinLength} characters.")
            .MaximumLength(Slug.MaxLength)
            .WithMessage($"Slug cannot exceed {Slug.MaxLength} characters.")
            .Must(Slug.IsValid)
            .WithMessage("Slug must be in a valid format (lowercase letters, numbers, and hyphens only).");
    }
}
