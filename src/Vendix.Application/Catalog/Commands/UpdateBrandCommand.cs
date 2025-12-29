using MediatR;
using Vendix.Application.Common.Exceptions;
using Vendix.Application.Common.Interfaces;
using Vendix.Application.Common.Models;
using Vendix.Domain.Catalog.Repositories;
using Vendix.Domain.Catalog.ValueObjects;

namespace Vendix.Application.Catalog.Commands;

public record UpdateBrandCommand : IRequest<Result>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Slug { get; init; }
    public string? LogoUrl { get; init; }
}

public class UpdateBrandCommandHandler(
    IBrandRepository brandRepository,
    IUnitOfWork unitOfWork,
    ICacheService cacheService) : IRequestHandler<UpdateBrandCommand, Result>
{
    public async Task<Result> Handle(UpdateBrandCommand request, CancellationToken cancellationToken)
    {
        var brand = await brandRepository.GetByIdAsync(request.Id, cancellationToken);
        if (brand is null)
        {
            throw new NotFoundException("Brand", request.Id);
        }

        // Generate slug if not provided
        var slugValue = string.IsNullOrWhiteSpace(request.Slug) 
            ? Slug.FromText(request.Name) 
            : new Slug(request.Slug);

        // Check for duplicate slug (excluding current brand)
        var existingBySlug = await brandRepository.GetBySlugAsync(slugValue.Value, cancellationToken);
        if (existingBySlug is not null && existingBySlug.Id != request.Id)
        {
            throw new ConflictException("Brand", "Slug", slugValue.Value);
        }

        // Update brand
        brand.UpdateName(request.Name);
        brand.UpdateSlug(slugValue);
        brand.UpdateLogoUrl(request.LogoUrl);

        brandRepository.Update(brand);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        await cacheService.RemoveByPrefixAsync("brands", cancellationToken);

        return Result.Success();
    }
}

