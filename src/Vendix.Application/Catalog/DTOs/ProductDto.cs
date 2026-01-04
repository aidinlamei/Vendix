using Vendix.Domain.Catalog.Enums;

namespace Vendix.Application.Catalog.DTOs;

/// <summary>
/// Full product data transfer object for detail views.
/// </summary>
public sealed record ProductDto
{
    /// <summary>
    /// Gets the unique identifier of the product.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the product name.
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    /// Gets the product title (alias for Name, used for translations).
    /// </summary>
    public string Title => Name;

    /// <summary>
    /// Gets the product description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the Stock Keeping Unit.
    /// </summary>
    public string Sku { get; init; } = null!;

    /// <summary>
    /// Gets the URL-friendly slug.
    /// </summary>
    public string Slug { get; init; } = null!;

    /// <summary>
    /// Gets the product price amount.
    /// </summary>
    public decimal Price { get; init; }

    /// <summary>
    /// Gets the product price currency.
    /// </summary>
    public string Currency { get; init; } = null!;

    /// <summary>
    /// Gets the product type.
    /// </summary>
    public ProductType ProductType { get; init; }

    /// <summary>
    /// Gets the category ID.
    /// </summary>
    public Guid? CategoryId { get; init; }

    /// <summary>
    /// Gets the category name.
    /// </summary>
    public string? CategoryName { get; init; }

    /// <summary>
    /// Gets the category slug.
    /// </summary>
    public string? CategorySlug { get; init; }

    /// <summary>
    /// Gets the brand ID.
    /// </summary>
    public Guid? BrandId { get; init; }

    /// <summary>
    /// Gets the brand name.
    /// </summary>
    public string? BrandName { get; init; }

    /// <summary>
    /// Gets the brand slug.
    /// </summary>
    public string? BrandSlug { get; init; }

    /// <summary>
    /// Gets the main image URL.
    /// </summary>
    public string? MainImageUrl { get; init; }

    /// <summary>
    /// Gets the product variants.
    /// </summary>
    public IReadOnlyList<ProductVariantDto> Variants { get; init; } = [];

    /// <summary>
    /// Gets the product specifications.
    /// </summary>
    public IReadOnlyList<ProductSpecificationDto> Specifications { get; init; } = [];

    /// <summary>
    /// Gets the product images.
    /// </summary>
    public IReadOnlyList<ProductImageDto> Images { get; init; } = [];

    /// <summary>
    /// Gets the product translations.
    /// </summary>
    public IReadOnlyList<ProductTranslationDto> Translations { get; init; } = [];

    /// <summary>
    /// Gets the creation date.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Gets the last modification date.
    /// </summary>
    public DateTime? ModifiedAt { get; init; }

    /// <summary>
    /// Gets a value indicating whether the product is active (not deleted).
    /// </summary>
    public bool IsActive { get; init; }
}

/// <summary>
/// Minimal product data transfer object for list views.
/// </summary>
public sealed record ProductListDto
{
    /// <summary>
    /// Gets the unique identifier of the product.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the product name.
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    /// Gets the product title (alias for Name, used for translations).
    /// </summary>
    public string Title => Name;

    /// <summary>
    /// Gets the Stock Keeping Unit.
    /// </summary>
    public string Sku { get; init; } = null!;

    /// <summary>
    /// Gets the URL-friendly slug.
    /// </summary>
    public string Slug { get; init; } = null!;

    /// <summary>
    /// Gets the product price amount.
    /// </summary>
    public decimal Price { get; init; }

    /// <summary>
    /// Gets the product price currency.
    /// </summary>
    public string Currency { get; init; } = null!;

    /// <summary>
    /// Gets the main image URL.
    /// </summary>
    public string? MainImageUrl { get; init; }

    /// <summary>
    /// Gets the category name.
    /// </summary>
    public string? CategoryName { get; init; }

    /// <summary>
    /// Gets the category slug.
    /// </summary>
    public string? CategorySlug { get; init; }

    /// <summary>
    /// Gets the brand name.
    /// </summary>
    public string? BrandName { get; init; }

    /// <summary>
    /// Gets the brand slug.
    /// </summary>
    public string? BrandSlug { get; init; }

    /// <summary>
    /// Gets the total stock quantity (sum of all variant stocks).
    /// </summary>
    public int TotalStock { get; init; }

    /// <summary>
    /// Gets a value indicating whether the product is active (not deleted).
    /// </summary>
    public bool IsActive { get; init; }
}

/// <summary>
/// Data transfer object for product variant.
/// </summary>
public sealed record ProductVariantDto
{
    /// <summary>
    /// Gets the variant ID.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the variant SKU.
    /// </summary>
    public string Sku { get; init; } = null!;

    /// <summary>
    /// Gets the variant name.
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    /// Gets the price adjustment amount.
    /// </summary>
    public decimal PriceAdjustmentAmount { get; init; }

    /// <summary>
    /// Gets the price adjustment currency.
    /// </summary>
    public string PriceAdjustmentCurrency { get; init; } = null!;

    /// <summary>
    /// Gets the stock quantity.
    /// </summary>
    public int StockQuantity { get; init; }
}

/// <summary>
/// Data transfer object for product specification.
/// </summary>
public sealed record ProductSpecificationDto
{
    /// <summary>
    /// Gets the specification ID.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the specification name.
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    /// Gets the specification value.
    /// </summary>
    public string Value { get; init; } = null!;
}

/// <summary>
/// Data transfer object for product translation.
/// </summary>
public sealed record ProductTranslationDto
{
    /// <summary>
    /// Gets the translation ID.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the ISO 639-1 language code (e.g., "en", "fa").
    /// </summary>
    public string LanguageCode { get; init; } = null!;

    /// <summary>
    /// Gets the translated product title.
    /// </summary>
    public string Title { get; init; } = null!;

    /// <summary>
    /// Gets the translated product description.
    /// </summary>
    public string? Description { get; init; }
}

/// <summary>
/// Data transfer object for product image.
/// </summary>
public sealed record ProductImageDto
{
    /// <summary>
    /// Gets the image ID.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the image URL.
    /// </summary>
    public string Url { get; init; } = null!;

    /// <summary>
    /// Gets the alt text.
    /// </summary>
    public string? AltText { get; init; }

    /// <summary>
    /// Gets the sort order.
    /// </summary>
    public int SortOrder { get; init; }

    /// <summary>
    /// Gets a value indicating whether this is the main image.
    /// </summary>
    public bool IsMain { get; init; }
}

/// <summary>
/// Data transfer object for creating a product.
/// </summary>
public sealed record CreateProductDto
{
    /// <summary>
    /// Gets or sets the product name.
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    /// Gets or sets the Stock Keeping Unit.
    /// </summary>
    public string Sku { get; init; } = null!;

    /// <summary>
    /// Gets or sets the URL-friendly slug.
    /// </summary>
    public string Slug { get; init; } = null!;

    /// <summary>
    /// Gets or sets the product price.
    /// </summary>
    public decimal Price { get; init; }

    /// <summary>
    /// Gets or sets the product currency.
    /// </summary>
    public string Currency { get; init; } = null!;

    /// <summary>
    /// Gets or sets the product type.
    /// </summary>
    public ProductType ProductType { get; init; }

    /// <summary>
    /// Gets or sets the product description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets or sets the category ID.
    /// </summary>
    public Guid? CategoryId { get; init; }

    /// <summary>
    /// Gets or sets the brand ID.
    /// </summary>
    public Guid? BrandId { get; init; }
}

/// <summary>
/// Data transfer object for updating a product.
/// </summary>
public sealed record UpdateProductDto
{
    /// <summary>
    /// Gets or sets the product ID.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the product name.
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    /// Gets or sets the Stock Keeping Unit.
    /// </summary>
    public string Sku { get; init; } = null!;

    /// <summary>
    /// Gets or sets the URL-friendly slug.
    /// </summary>
    public string Slug { get; init; } = null!;

    /// <summary>
    /// Gets or sets the product price.
    /// </summary>
    public decimal Price { get; init; }

    /// <summary>
    /// Gets or sets the product currency.
    /// </summary>
    public string Currency { get; init; } = null!;

    /// <summary>
    /// Gets or sets the product type.
    /// </summary>
    public ProductType ProductType { get; init; }

    /// <summary>
    /// Gets or sets the product description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets or sets the category ID.
    /// </summary>
    public Guid? CategoryId { get; init; }

    /// <summary>
    /// Gets or sets the brand ID.
    /// </summary>
    public Guid? BrandId { get; init; }
}
