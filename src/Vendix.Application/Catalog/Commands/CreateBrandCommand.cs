using MediatR;
using Vendix.Application.Common.Exceptions;
using Vendix.Application.Common.Interfaces;
using Vendix.Application.Common.Models;
using Vendix.Domain.Catalog.Entities;
using Vendix.Domain.Catalog.Repositories;
using Vendix.Domain.Catalog.ValueObjects;

namespace Vendix.Application.Catalog.Commands;

public record CreateBrandCommand : IRequest<Result<Guid>>
{
    public string Name { get; init; } = string.Empty;
    public string? Slug { get; init; }
    public string? LogoUrl { get; init; }
}

public class CreateBrandCommandHandler(
    IBrandRepository brandRepository,
    IUnitOfWork unitOfWork,
    ICacheService cacheService) : IRequestHandler<CreateBrandCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateBrandCommand request, CancellationToken cancellationToken)
    {
        // Generate slug if not provided
        var slugValue = string.IsNullOrWhiteSpace(request.Slug) 
            ? Slug.FromText(request.Name) 
            : new Slug(request.Slug);

        // Check for duplicate slug
        var existingBySlug = await brandRepository.GetBySlugAsync(slugValue.Value, cancellationToken);
        if (existingBySlug is not null)
        {
            throw new ConflictException("Brand", "Slug", slugValue.Value);
        }

        // Create brand
        var brand = new Brand(request.Name, slugValue, request.LogoUrl);

        await brandRepository.AddAsync(brand, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        await cacheService.RemoveByPrefixAsync("brands", cancellationToken);

        return Result<Guid>.Success(brand.Id);
    }
}

