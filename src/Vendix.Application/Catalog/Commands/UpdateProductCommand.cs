using FluentValidation;
using MediatR;
using Vendix.Application.Common.Exceptions;
using Vendix.Application.Common.Interfaces;
using Vendix.Application.Common.Models;
using Vendix.Domain.Catalog.Enums;
using Vendix.Domain.Catalog.Repositories;
using Vendix.Domain.Catalog.ValueObjects;

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
    IReadOnlyList<ProductTranslationInput>? Translations = null) : IRequest<Result>;

/// <summary>
/// Handler for <see cref="UpdateProductCommand"/>.
/// </summary>
public sealed class UpdateProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateProductCommand, Result>
{
    /// <inheritdoc />
    public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null)
        {
            throw NotFoundException.ForEntity<Domain.Catalog.Entities.Product>(request.Id);
        }

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
        await unitOfWork.SaveChangesAsync(cancellationToken);

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
