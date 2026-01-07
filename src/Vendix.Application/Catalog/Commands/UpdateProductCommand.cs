using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vendix.Application.Common.Exceptions;
using Vendix.Application.Common.Interfaces;
using Vendix.Application.Common.Models;
using Vendix.Domain.Catalog.Enums;
using Vendix.Domain.Catalog.Repositories;
using Vendix.Domain.Catalog.ValueObjects;

namespace Vendix.Application.Catalog.Commands;

public sealed record UpdateProductCommand(
    Guid Id,
    string Name,
    string Slug,
    decimal Price,
    string Currency,
    ProductType ProductType,
    string? Description = null,
    Guid? CategoryId = null,
    Guid? BrandId = null,
    bool IsActive = true,
    IReadOnlyList<ProductTranslationInput>? Translations = null,
    IReadOnlyList<ProductImageInput>? Images = null) : IRequest<Result>;

public sealed class UpdateProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    ICacheService cacheService) : IRequestHandler<UpdateProductCommand, Result>
{
    public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null)
        {
            throw NotFoundException.ForEntity<Domain.Catalog.Entities.Product>(request.Id);
        }

        var existingBySlug = await productRepository.GetBySlugAsync(
            new Slug(request.Slug), cancellationToken);
        if (existingBySlug is not null && existingBySlug.Id != request.Id)
        {
            throw new ConflictException("Product", "Slug", request.Slug);
        }

        product.UpdateName(request.Name);
        product.UpdateSlug(new Slug(request.Slug));
        product.UpdatePrice(new Money(request.Price, request.Currency));
        product.UpdateProductType(request.ProductType);
        product.UpdateDescription(request.Description);
        product.AssignToCategory(request.CategoryId);
        product.AssignToBrand(request.BrandId);

        if (request.Translations is not null)
        {
            foreach (var translation in request.Translations)
            {
                product.RemoveTranslation(translation.LanguageCode);
                product.AddTranslation(
                    translation.LanguageCode,
                    translation.Title,
                    translation.Description);
            }
        }

        if (request.Images is not null)
        {
            var existingImageIds = request.Images
                .Where(img => img.Id.HasValue)
                .Select(img => img.Id!.Value)
                .ToHashSet();

            var imagesToRemove = product.Images
                .Where(img => !existingImageIds.Contains(img.Id))
                .Select(img => img.Id)
                .ToList();
            
            foreach (var imageId in imagesToRemove)
            {
                product.RemoveImage(imageId);
            }

            foreach (var imageInput in request.Images)
            {
                if (imageInput.Id.HasValue)
                {
                    var existingImage = product.Images.FirstOrDefault(i => i.Id == imageInput.Id.Value);
                    if (existingImage != null)
                    {
                        existingImage.UpdateUrl(imageInput.Url);
                        existingImage.UpdateAltText(imageInput.AltText);
                        existingImage.UpdateSortOrder(imageInput.SortOrder);
                        
                        if (imageInput.IsMain && !existingImage.IsMain)
                        {
                            existingImage.SetAsMain();
                        }
                        else if (!imageInput.IsMain && existingImage.IsMain)
                        {
                            existingImage.UnsetAsMain();
                        }
                    }
                }
                else
                {
                    product.AddImage(
                        imageInput.Url,
                        imageInput.AltText,
                        imageInput.SortOrder,
                        imageInput.IsMain);
                }
            }
        }

        if (request.IsActive && product.IsDeleted)
        {
            product.IsDeleted = false;
            product.DeletedAt = null;
            product.DeletedBy = null;
        }
        else if (!request.IsActive && !product.IsDeleted)
        {
            product.MarkAsDeleted();
        }

        productRepository.Update(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await cacheService.RemoveByPrefixAsync("products", cancellationToken);

        return Result.Success();
    }
}

public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Product ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Product name is required.")
            .MaximumLength(200)
            .WithMessage("Product name must not exceed 200 characters.");

        RuleFor(x => x.Slug)
            .NotEmpty()
            .WithMessage("Slug is required.")
            .MinimumLength(Slug.MinLength)
            .WithMessage($"Slug must be at least {Slug.MinLength} characters.")
            .MaximumLength(Slug.MaxLength)
            .WithMessage($"Slug must not exceed {Slug.MaxLength} characters.")
            .Matches(Slug.Pattern)
            .WithMessage("Slug must contain only lowercase letters, numbers, and hyphens.");

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("Price must be greater than zero.");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Currency is required.")
            .Length(3)
            .WithMessage("Currency must be a 3-character code.");

        RuleFor(x => x.ProductType)
            .IsInEnum()
            .WithMessage("Invalid product type.");

        RuleFor(x => x.Description)
            .MaximumLength(4000)
            .WithMessage("Description must not exceed 4000 characters.")
            .When(x => x.Description is not null);
    }
}
