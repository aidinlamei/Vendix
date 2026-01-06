using FluentValidation;
using MediatR;
using Vendix.Application.Common.Exceptions;
using Vendix.Application.Common.Interfaces;
using Vendix.Application.Common.Models;
using Vendix.Domain.Catalog.Entities;
using Vendix.Domain.Catalog.Enums;
using Vendix.Domain.Catalog.Repositories;
using Vendix.Domain.Catalog.ValueObjects;

namespace Vendix.Application.Catalog.Commands;

/// <summary>
/// Input model for product translation.
/// </summary>
public sealed class ProductTranslationInput
{
    /// <summary>
    /// Gets or sets the ISO 639-1 language code (e.g., "en", "fa").
    /// </summary>
    public string LanguageCode { get; set; } = "en";

    /// <summary>
    /// Gets or sets the translated product title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the translated product description.
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Input model for product image.
/// </summary>
public sealed class ProductImageInput
{
    /// <summary>
    /// Gets or sets the image ID (for existing images) or null for new images.
    /// </summary>
    public Guid? Id { get; set; }

    /// <summary>
    /// Gets or sets the image URL.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the alternative text for accessibility.
    /// </summary>
    public string? AltText { get; set; }

    /// <summary>
    /// Gets or sets the sort order for display sequence.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is the main product image.
    /// </summary>
    public bool IsMain { get; set; }
}

/// <summary>
/// Command to create a new product.
/// </summary>
/// <param name="Name">The product name.</param>
/// <param name="Sku">The Stock Keeping Unit.</param>
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
public sealed record CreateProductCommand(
    string Name,
    string Sku,
    string? Slug,
    decimal Price,
    string Currency,
    ProductType ProductType,
    string? Description = null,
    Guid? CategoryId = null,
    Guid? BrandId = null,
    bool IsActive = true,
    IReadOnlyList<ProductTranslationInput>? Translations = null,
    IReadOnlyList<ProductImageInput>? Images = null) : IRequest<Result<Guid>>;

/// <summary>
/// Handler for <see cref="CreateProductCommand"/>.
/// </summary>
public sealed class CreateProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    ICacheService cacheService) : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    /// <inheritdoc />
    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // Generate slug if not provided
        var slug = request.Slug ?? GenerateSlug(request.Name);
        if (string.IsNullOrWhiteSpace(slug))
        {
            slug = GenerateSlug(request.Name);
        }

        // Check for duplicate Slug
        var existingBySlug = await productRepository.GetBySlugAsync(
            new Slug(slug), cancellationToken);
        if (existingBySlug is not null)
        {
            throw new ConflictException("Product", "Slug", slug);
        }

        var product = new Product(
            request.Name,
            new Sku(request.Sku),
            new Slug(slug),
            new Money(request.Price, request.Currency),
            request.ProductType,
            request.Description,
            request.CategoryId,
            request.BrandId);

        // Add translations if provided
        if (request.Translations is not null)
        {
            foreach (var translation in request.Translations)
            {
                product.AddTranslation(
                    translation.LanguageCode,
                    translation.Title,
                    translation.Description);
            }
        }

        // Add images if provided
        if (request.Images is not null && request.Images.Any())
        {
            foreach (var image in request.Images)
            {
                product.AddImage(
                    image.Url,
                    image.AltText,
                    image.SortOrder,
                    image.IsMain);
            }
        }

        // Soft delete if not active
        if (!request.IsActive)
        {
            product.MarkAsDeleted();
        }

        await productRepository.AddAsync(product, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        await cacheService.RemoveByPrefixAsync("products", cancellationToken);

        return Result<Guid>.Success(product.Id);
    }

    private static string GenerateSlug(string name)
    {
        // Simple slug generation - convert to lowercase, replace spaces with hyphens
        return name.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("_", "-")
            .Trim('-');
    }
}

/// <summary>
/// Validator for <see cref="CreateProductCommand"/>.
/// </summary>
public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateProductCommandValidator"/> class.
    /// </summary>
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Product name is required.")
            .MaximumLength(200)
            .WithMessage("Product name must not exceed 200 characters.");

        RuleFor(x => x.Sku)
            .NotEmpty()
            .WithMessage("SKU is required.")
            .MinimumLength(Sku.MinLength)
            .WithMessage($"SKU must be at least {Sku.MinLength} characters.")
            .MaximumLength(Sku.MaxLength)
            .WithMessage($"SKU must not exceed {Sku.MaxLength} characters.")
            .Matches(Sku.Pattern)
            .WithMessage("SKU must contain only letters, numbers, and hyphens.");

        RuleFor(x => x.Slug)
            .MinimumLength(Slug.MinLength)
            .WithMessage($"Slug must be at least {Slug.MinLength} characters.")
            .MaximumLength(Slug.MaxLength)
            .WithMessage($"Slug must not exceed {Slug.MaxLength} characters.")
            .Matches(Slug.Pattern)
            .WithMessage("Slug must contain only lowercase letters, numbers, and hyphens.")
            .When(x => !string.IsNullOrWhiteSpace(x.Slug));

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
