using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vendix.Application.Common.Exceptions;
using Vendix.Application.Common.Interfaces;
using Vendix.Application.Common.Models;
using Vendix.Domain.Catalog.Enums;
using Vendix.Domain.Catalog.Repositories;
using Vendix.Domain.Catalog.ValueObjects;
using Vendix.Application.Catalog.Commands;

namespace Vendix.Application.Catalog.Commands;

/// <summary>
/// Command to update an existing product.
/// </summary>
/// <param name="Id">The product ID.</param>
/// <param name="Name">The product name.</param>
/// <param name="Slug">The URL-friendly slug.</param>
/// <param name="Price">The product price.</param>
/// <param name="Currency">The price currency.</param>
/// <param name="ProductType">The product type.</param>
/// <param name="Description">Optional product description.</param>
/// <param name="CategoryId">Optional category ID.</param>
/// <param name="BrandId">Optional brand ID.</param>
/// <param name="IsActive">Whether the product is active (visible to customers).</param>
/// <param name="Translations">Optional list of translations.</param>
/// <param name="Images">Optional list of product images.</param>
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

/// <summary>
/// Handler for <see cref="UpdateProductCommand"/>.
/// </summary>
public sealed class UpdateProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    ICacheService cacheService) : IRequestHandler<UpdateProductCommand, Result>
{
    /// <inheritdoc />
    public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null)
        {
            throw NotFoundException.ForEntity<Domain.Catalog.Entities.Product>(request.Id);
        }
        
        // Debug: Log initial RowVersion
        System.Diagnostics.Debug.WriteLine($"[UpdateProductCommand] Initial Product RowVersion: {(product.RowVersion != null ? Convert.ToHexString(product.RowVersion) : "null")}");

        // Check for duplicate slug (excluding current product)
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

        // Update translations
        if (request.Translations is not null)
        {
            // Remove existing translations for languages being updated
            foreach (var translation in request.Translations)
            {
                product.RemoveTranslation(translation.LanguageCode);
                product.AddTranslation(
                    translation.LanguageCode,
                    translation.Title,
                    translation.Description);
            }
        }

        // Update images
        if (request.Images is not null)
        {
            // Get existing image IDs from request
            var existingImageIds = request.Images
                .Where(img => img.Id.HasValue)
                .Select(img => img.Id!.Value)
                .ToHashSet();

            // Remove images that are not in the request
            var imagesToRemove = product.Images
                .Where(img => !existingImageIds.Contains(img.Id))
                .Select(img => img.Id)
                .ToList();
            
            foreach (var imageId in imagesToRemove)
            {
                product.RemoveImage(imageId);
            }

            // Update existing images or add new ones
            foreach (var imageInput in request.Images)
            {
                if (imageInput.Id.HasValue)
                {
                    // Update existing image
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
                    // Add new image
                    product.AddImage(
                        imageInput.Url,
                        imageInput.AltText,
                        imageInput.SortOrder,
                        imageInput.IsMain);
                }
            }
        }

        // Update active status
        if (request.IsActive && product.IsDeleted)
        {
            // Restore if was deleted
            product.IsDeleted = false;
            product.DeletedAt = null;
            product.DeletedBy = null;
        }
        else if (!request.IsActive && !product.IsDeleted)
        {
            product.MarkAsDeleted();
        }

        productRepository.Update(product);
        
        // Debug: Log RowVersion before save
        System.Diagnostics.Debug.WriteLine($"[UpdateProductCommand] Before SaveChanges - Product RowVersion: {(product.RowVersion != null ? Convert.ToHexString(product.RowVersion) : "null")}");
        
        // Debug: Log that we're about to save
        System.Diagnostics.Debug.WriteLine($"[UpdateProductCommand] About to save changes for product {product.Id}");
        
        try
        {
            var rowsAffected = await unitOfWork.SaveChangesAsync(cancellationToken);
            System.Diagnostics.Debug.WriteLine($"[UpdateProductCommand] SaveChanges succeeded, rows affected: {rowsAffected}");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[UpdateProductCommand] Concurrency exception caught: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[UpdateProductCommand] Failed entries count: {ex.Entries.Count}");
            foreach (var entry in ex.Entries)
            {
                System.Diagnostics.Debug.WriteLine($"[UpdateProductCommand] Failed entry: {entry.Entity.GetType().Name}, Id: {entry.Entity.GetType().GetProperty("Id")?.GetValue(entry.Entity)}");
                if (entry.Entity is Domain.Catalog.Entities.Product failedProduct)
                {
                    System.Diagnostics.Debug.WriteLine($"[UpdateProductCommand] Failed Product RowVersion: {(failedProduct.RowVersion != null ? Convert.ToHexString(failedProduct.RowVersion) : "null")}");
                    var rowVersionProp = entry.Property("RowVersion");
                    var failedOriginal = rowVersionProp.OriginalValue as byte[];
                    var failedCurrent = rowVersionProp.CurrentValue as byte[];
                    System.Diagnostics.Debug.WriteLine($"[UpdateProductCommand] Failed Product Original RowVersion: {(failedOriginal != null ? Convert.ToHexString(failedOriginal) : "null")}");
                    System.Diagnostics.Debug.WriteLine($"[UpdateProductCommand] Failed Product Current RowVersion: {(failedCurrent != null ? Convert.ToHexString(failedCurrent) : "null")}");
                }
            }
            
            // Reload product with fresh RowVersion from database
            // Use GetByIdAsync which will load with current RowVersion
            var reloadedProduct = await productRepository.GetByIdAsync(request.Id, cancellationToken);
            if (reloadedProduct is null)
            {
                throw NotFoundException.ForEntity<Domain.Catalog.Entities.Product>(request.Id);
            }
            
            System.Diagnostics.Debug.WriteLine($"[UpdateProductCommand] Reloaded Product RowVersion: {(reloadedProduct.RowVersion != null ? Convert.ToHexString(reloadedProduct.RowVersion) : "null")}");
            
            // The reloaded product has the current RowVersion from database
            // This will be used as OriginalValues when we call Update

            // Reapply changes to reloaded product
            reloadedProduct.UpdateName(request.Name);
            reloadedProduct.UpdateSlug(new Slug(request.Slug));
            reloadedProduct.UpdatePrice(new Money(request.Price, request.Currency));
            reloadedProduct.UpdateProductType(request.ProductType);
            reloadedProduct.UpdateDescription(request.Description);
            reloadedProduct.AssignToCategory(request.CategoryId);
            reloadedProduct.AssignToBrand(request.BrandId);

            // Update translations
            if (request.Translations is not null)
            {
                foreach (var translation in request.Translations)
                {
                    reloadedProduct.RemoveTranslation(translation.LanguageCode);
                    reloadedProduct.AddTranslation(
                        translation.LanguageCode,
                        translation.Title,
                        translation.Description);
                }
            }

            // Update images
            if (request.Images is not null)
            {
                var existingImageIds = request.Images
                    .Where(img => img.Id.HasValue)
                    .Select(img => img.Id!.Value)
                    .ToHashSet();

                var imagesToRemove = reloadedProduct.Images
                    .Where(img => !existingImageIds.Contains(img.Id))
                    .Select(img => img.Id)
                    .ToList();
                
                foreach (var imageId in imagesToRemove)
                {
                    reloadedProduct.RemoveImage(imageId);
                }

                foreach (var imageInput in request.Images)
                {
                    if (imageInput.Id.HasValue)
                    {
                        var existingImage = reloadedProduct.Images.FirstOrDefault(i => i.Id == imageInput.Id.Value);
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
                        reloadedProduct.AddImage(
                            imageInput.Url,
                            imageInput.AltText,
                            imageInput.SortOrder,
                            imageInput.IsMain);
                    }
                }
            }

            // Update active status
            if (request.IsActive && reloadedProduct.IsDeleted)
            {
                reloadedProduct.IsDeleted = false;
                reloadedProduct.DeletedAt = null;
                reloadedProduct.DeletedBy = null;
            }
            else if (!request.IsActive && !reloadedProduct.IsDeleted)
            {
                reloadedProduct.MarkAsDeleted();
            }

            // Update the reloaded product - this will detach any previously tracked entity
            productRepository.Update(reloadedProduct);
            
            System.Diagnostics.Debug.WriteLine($"[UpdateProductCommand] Before retry SaveChanges - Reloaded Product RowVersion: {(reloadedProduct.RowVersion != null ? Convert.ToHexString(reloadedProduct.RowVersion) : "null")}");
            
            // Retry save with fresh entity and updated RowVersion (will be set by interceptor)
            await unitOfWork.SaveChangesAsync(cancellationToken);
            
            System.Diagnostics.Debug.WriteLine($"[UpdateProductCommand] Retry SaveChanges succeeded");
        }

        // Invalidate cache
        await cacheService.RemoveByPrefixAsync("products", cancellationToken);

        return Result.Success();
    }
}

/// <summary>
/// Validator for <see cref="UpdateProductCommand"/>.
/// </summary>
public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateProductCommandValidator"/> class.
    /// </summary>
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
