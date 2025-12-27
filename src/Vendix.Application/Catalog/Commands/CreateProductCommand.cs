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
public sealed record CreateProductCommand(
    string Name,
    string Sku,
    string Slug,
    decimal Price,
    string Currency,
    ProductType ProductType,
    string? Description = null,
    Guid? CategoryId = null,
    Guid? BrandId = null) : IRequest<Result<Guid>>;

/// <summary>
/// Handler for <see cref="CreateProductCommand"/>.
/// </summary>
public sealed class CreateProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    /// <inheritdoc />
    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // Check for duplicate SKU
        var existingSku = await productRepository.GetBySlugAsync(
            new Slug(request.Slug), cancellationToken);
        if (existingSku is not null)
        {
            throw new ConflictException("Product", "Slug", request.Slug);
        }

        var product = new Product(
            request.Name,
            new Sku(request.Sku),
            new Slug(request.Slug),
            new Money(request.Price, request.Currency),
            request.ProductType,
            request.Description,
            request.CategoryId,
            request.BrandId);

        await productRepository.AddAsync(product, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(product.Id);
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
