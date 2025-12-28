using Vendix.Domain.Catalog.Enums;
using Vendix.Domain.Catalog.ValueObjects;
using Vendix.Domain.Common;

namespace Vendix.Domain.Catalog.Entities;

/// <summary>
/// Represents a product in the catalog.
/// </summary>
/// <remarks>
/// Product is an aggregate root that encapsulates all product-related entities including
/// variants, specifications, images, and translations. It maintains consistency across
/// all child entities and enforces business rules for the product aggregate.
/// </remarks>
public class Product : AggregateRoot, IAuditableEntity, ISoftDelete
{
    private readonly List<ProductVariant> _variants = [];
    private readonly List<ProductSpecification> _specifications = [];
    private readonly List<ProductImage> _images = [];
    private readonly List<ProductTranslation> _translations = [];

    /// <summary>
    /// Gets the product name.
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Gets the product description.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Gets the Stock Keeping Unit for this product.
    /// </summary>
    public Sku Sku { get; private set; } = null!;

    /// <summary>
    /// Gets the URL-friendly slug for the product.
    /// </summary>
    public Slug Slug { get; private set; } = null!;

    /// <summary>
    /// Gets the base price of the product.
    /// </summary>
    public Money Price { get; private set; } = null!;

    /// <summary>
    /// Gets the type of product (Physical or Digital).
    /// </summary>
    public ProductType ProductType { get; private set; }

    /// <summary>
    /// Gets the ID of the category this product belongs to.
    /// </summary>
    public Guid? CategoryId { get; private set; }

    /// <summary>
    /// Gets the ID of the brand for this product.
    /// </summary>
    public Guid? BrandId { get; private set; }

    /// <summary>
    /// Gets the category navigation property.
    /// </summary>
    public Category? Category { get; private set; }

    /// <summary>
    /// Gets the brand navigation property.
    /// </summary>
    public Brand? Brand { get; private set; }

    /// <summary>
    /// Gets the product variants.
    /// </summary>
    public IReadOnlyCollection<ProductVariant> Variants => _variants.AsReadOnly();

    /// <summary>
    /// Gets the product specifications.
    /// </summary>
    public IReadOnlyCollection<ProductSpecification> Specifications => _specifications.AsReadOnly();

    /// <summary>
    /// Gets the product images.
    /// </summary>
    public IReadOnlyCollection<ProductImage> Images => _images.AsReadOnly();

    /// <summary>
    /// Gets the product translations.
    /// </summary>
    public IReadOnlyCollection<ProductTranslation> Translations => _translations.AsReadOnly();

    /// <summary>
    /// Gets or sets the date and time when this entity was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who created this entity.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this entity was last modified.
    /// </summary>
    public DateTime? ModifiedAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who last modified this entity.
    /// </summary>
    public string? ModifiedBy { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this entity has been soft deleted.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this entity was soft deleted.
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who deleted this entity.
    /// </summary>
    public string? DeletedBy { get; set; }

    /// <summary>
    /// Required by EF Core for materialization.
    /// </summary>
    private Product() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Product"/> class.
    /// </summary>
    /// <param name="name">The product name.</param>
    /// <param name="sku">The Stock Keeping Unit.</param>
    /// <param name="slug">The URL-friendly slug.</param>
    /// <param name="price">The base price.</param>
    /// <param name="productType">The type of product.</param>
    /// <param name="description">Optional product description.</param>
    /// <param name="categoryId">Optional category ID.</param>
    /// <param name="brandId">Optional brand ID.</param>
    /// <exception cref="ArgumentException">Thrown when name is null or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown when sku, slug, or price is null.</exception>
    public Product(
        string name,
        Sku sku,
        Slug slug,
        Money price,
        ProductType productType,
        string? description = null,
        Guid? categoryId = null,
        Guid? brandId = null)
    {
        SetName(name);
        SetSku(sku);
        SetSlug(slug);
        SetPrice(price);
        ProductType = productType;
        Description = description?.Trim();
        CategoryId = categoryId;
        BrandId = brandId;
    }

    /// <summary>
    /// Updates the product name.
    /// </summary>
    /// <param name="name">The new product name.</param>
    /// <exception cref="ArgumentException">Thrown when name is null or whitespace.</exception>
    public void UpdateName(string name)
    {
        SetName(name);
    }

    /// <summary>
    /// Updates the product description.
    /// </summary>
    /// <param name="description">The new description, or null to remove.</param>
    public void UpdateDescription(string? description)
    {
        Description = description?.Trim();
    }

    /// <summary>
    /// Updates the product SKU.
    /// </summary>
    /// <param name="sku">The new SKU.</param>
    /// <exception cref="ArgumentNullException">Thrown when sku is null.</exception>
    public void UpdateSku(Sku sku)
    {
        SetSku(sku);
    }

    /// <summary>
    /// Updates the product slug.
    /// </summary>
    /// <param name="slug">The new slug.</param>
    /// <exception cref="ArgumentNullException">Thrown when slug is null.</exception>
    public void UpdateSlug(Slug slug)
    {
        SetSlug(slug);
    }

    /// <summary>
    /// Updates the product price.
    /// </summary>
    /// <param name="price">The new price.</param>
    /// <exception cref="ArgumentNullException">Thrown when price is null.</exception>
    public void UpdatePrice(Money price)
    {
        SetPrice(price);
    }

    /// <summary>
    /// Updates the product type.
    /// </summary>
    /// <param name="productType">The new product type.</param>
    public void UpdateProductType(ProductType productType)
    {
        ProductType = productType;
    }

    /// <summary>
    /// Assigns this product to a category.
    /// </summary>
    /// <param name="categoryId">The category ID, or null to remove from category.</param>
    public void AssignToCategory(Guid? categoryId)
    {
        CategoryId = categoryId;
    }

    /// <summary>
    /// Assigns this product to a brand.
    /// </summary>
    /// <param name="brandId">The brand ID, or null to remove brand.</param>
    public void AssignToBrand(Guid? brandId)
    {
        BrandId = brandId;
    }

    /// <summary>
    /// Adds a variant to this product.
    /// </summary>
    /// <param name="sku">The variant SKU.</param>
    /// <param name="name">The variant name.</param>
    /// <param name="priceAdjustmentAmount">The price adjustment amount (can be negative for discounts).</param>
    /// <param name="priceAdjustmentCurrency">The currency code for the price adjustment.</param>
    /// <param name="stockQuantity">The initial stock quantity.</param>
    /// <returns>The created variant.</returns>
    /// <exception cref="InvalidOperationException">Thrown when a variant with this SKU already exists.</exception>
    public ProductVariant AddVariant(Sku sku, string name, decimal priceAdjustmentAmount, string priceAdjustmentCurrency, int stockQuantity = 0)
    {
        if (_variants.Any(v => v.Sku == sku))
        {
            throw new InvalidOperationException($"A variant with SKU '{sku}' already exists.");
        }

        var variant = new ProductVariant(Id, sku, name, priceAdjustmentAmount, priceAdjustmentCurrency, stockQuantity);
        _variants.Add(variant);
        return variant;
    }

    /// <summary>
    /// Removes a variant from this product.
    /// </summary>
    /// <param name="variantId">The variant ID to remove.</param>
    /// <returns>True if the variant was removed; otherwise, false.</returns>
    public bool RemoveVariant(Guid variantId)
    {
        var variant = _variants.FirstOrDefault(v => v.Id == variantId);

        if (variant is null)
        {
            return false;
        }

        _variants.Remove(variant);
        return true;
    }

    /// <summary>
    /// Adds a specification to this product.
    /// </summary>
    /// <param name="name">The specification name.</param>
    /// <param name="value">The specification value.</param>
    /// <returns>The created specification.</returns>
    /// <exception cref="InvalidOperationException">Thrown when a specification with this name already exists.</exception>
    public ProductSpecification AddSpecification(string name, string value)
    {
        var normalizedName = name.Trim().ToLowerInvariant();

        if (_specifications.Any(s => s.Name.ToLowerInvariant() == normalizedName))
        {
            throw new InvalidOperationException($"A specification with name '{name}' already exists.");
        }

        var specification = new ProductSpecification(Id, name, value);
        _specifications.Add(specification);
        return specification;
    }

    /// <summary>
    /// Removes a specification from this product.
    /// </summary>
    /// <param name="specificationId">The specification ID to remove.</param>
    /// <returns>True if the specification was removed; otherwise, false.</returns>
    public bool RemoveSpecification(Guid specificationId)
    {
        var specification = _specifications.FirstOrDefault(s => s.Id == specificationId);

        if (specification is null)
        {
            return false;
        }

        _specifications.Remove(specification);
        return true;
    }

    /// <summary>
    /// Adds an image to this product.
    /// </summary>
    /// <param name="url">The image URL.</param>
    /// <param name="altText">Optional alt text for accessibility.</param>
    /// <param name="sortOrder">The display sort order.</param>
    /// <param name="isMain">Whether this is the main product image.</param>
    /// <returns>The created image.</returns>
    public ProductImage AddImage(string url, string? altText = null, int sortOrder = 0, bool isMain = false)
    {
        // If this is the main image, unset any existing main image
        if (isMain)
        {
            foreach (var existingImage in _images.Where(i => i.IsMain))
            {
                existingImage.UnsetAsMain();
            }
        }

        // If this is the first image, make it the main image
        if (!_images.Any())
        {
            isMain = true;
        }

        var image = new ProductImage(Id, url, altText, sortOrder, isMain);
        _images.Add(image);
        return image;
    }

    /// <summary>
    /// Removes an image from this product.
    /// </summary>
    /// <param name="imageId">The image ID to remove.</param>
    /// <returns>True if the image was removed; otherwise, false.</returns>
    public bool RemoveImage(Guid imageId)
    {
        var image = _images.FirstOrDefault(i => i.Id == imageId);

        if (image is null)
        {
            return false;
        }

        var wasMain = image.IsMain;
        _images.Remove(image);

        // If the removed image was the main one, set the first remaining image as main
        if (wasMain && _images.Any())
        {
            _images.First().SetAsMain();
        }

        return true;
    }

    /// <summary>
    /// Sets an image as the main product image.
    /// </summary>
    /// <param name="imageId">The image ID to set as main.</param>
    /// <exception cref="InvalidOperationException">Thrown when the image is not found.</exception>
    public void SetMainImage(Guid imageId)
    {
        var image = _images.FirstOrDefault(i => i.Id == imageId);

        if (image is null)
        {
            throw new InvalidOperationException($"Image with ID '{imageId}' not found.");
        }

        foreach (var existingImage in _images.Where(i => i.IsMain))
        {
            existingImage.UnsetAsMain();
        }

        image.SetAsMain();
    }

    /// <summary>
    /// Adds a translation for this product.
    /// </summary>
    /// <param name="languageCode">The ISO 639-1 language code.</param>
    /// <param name="title">The translated product title.</param>
    /// <param name="description">Optional translated description.</param>
    /// <returns>The created translation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when a translation for this language already exists.</exception>
    public ProductTranslation AddTranslation(string languageCode, string title, string? description = null)
    {
        var normalizedCode = languageCode.ToLowerInvariant();

        if (_translations.Any(t => t.LanguageCode == normalizedCode))
        {
            throw new InvalidOperationException($"A translation for language '{languageCode}' already exists.");
        }

        var translation = new ProductTranslation(Id, languageCode, title, description);
        _translations.Add(translation);
        return translation;
    }

    /// <summary>
    /// Gets the translation for a specific language.
    /// </summary>
    /// <param name="languageCode">The ISO 639-1 language code.</param>
    /// <returns>The translation if found; otherwise, null.</returns>
    public ProductTranslation? GetTranslation(string languageCode)
    {
        var normalizedCode = languageCode.ToLowerInvariant();
        return _translations.FirstOrDefault(t => t.LanguageCode == normalizedCode);
    }

    /// <summary>
    /// Gets the product title in the specified language, falling back to the default name.
    /// </summary>
    /// <param name="languageCode">The ISO 639-1 language code.</param>
    /// <returns>The translated title if available; otherwise, the default name.</returns>
    public string GetTitle(string languageCode)
    {
        return GetTranslation(languageCode)?.Title ?? Name;
    }

    /// <summary>
    /// Removes a translation for the specified language.
    /// </summary>
    /// <param name="languageCode">The ISO 639-1 language code.</param>
    /// <returns>True if the translation was removed; otherwise, false.</returns>
    public bool RemoveTranslation(string languageCode)
    {
        var normalizedCode = languageCode.ToLowerInvariant();
        var translation = _translations.FirstOrDefault(t => t.LanguageCode == normalizedCode);

        if (translation is null)
        {
            return false;
        }

        _translations.Remove(translation);
        return true;
    }

    /// <summary>
    /// Gets the main product image.
    /// </summary>
    /// <returns>The main image if found; otherwise, null.</returns>
    public ProductImage? GetMainImage()
    {
        return _images.FirstOrDefault(i => i.IsMain);
    }

    /// <inheritdoc />
    public void MarkAsDeleted()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Product name is required.", nameof(name));
        }

        Name = name.Trim();
    }

    private void SetSku(Sku sku)
    {
        ArgumentNullException.ThrowIfNull(sku);
        Sku = sku;
    }

    private void SetSlug(Slug slug)
    {
        ArgumentNullException.ThrowIfNull(slug);
        Slug = slug;
    }

    private void SetPrice(Money price)
    {
        ArgumentNullException.ThrowIfNull(price);
        Price = price;
    }
}
