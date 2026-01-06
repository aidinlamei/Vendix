using Mapster;
using Vendix.Application.Catalog.DTOs;
using Vendix.Domain.Catalog.Entities;

namespace Vendix.Application.Catalog.Mappings;

/// <summary>
/// Mapster mapping configuration for Product entities to DTOs.
/// </summary>
public sealed class ProductMappingConfig : IRegister
{
    /// <inheritdoc />
    public void Register(TypeAdapterConfig config)
    {
        // Product -> ProductDto
        config.NewConfig<Product, ProductDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Sku, src => src.Sku.Value)
            .Map(dest => dest.Slug, src => src.Slug.Value)
            .Map(dest => dest.Price, src => src.Price.Amount)
            .Map(dest => dest.Currency, src => src.Price.Currency)
            .Map(dest => dest.ProductType, src => src.ProductType)
            .Map(dest => dest.CategoryId, src => src.CategoryId)
            .Map(dest => dest.CategoryName, src => src.Category != null ? src.Category.Name : null)
            .Map(dest => dest.CategorySlug, src => src.Category != null ? src.Category.Slug.Value : null)
            .Map(dest => dest.BrandId, src => src.BrandId)
            .Map(dest => dest.BrandName, src => src.Brand != null ? src.Brand.Name : null)
            .Map(dest => dest.BrandSlug, src => src.Brand != null ? src.Brand.Slug.Value : null)
            .Map(dest => dest.MainImageUrl, src => GetMainImageUrl(src.Images))
            .Map(dest => dest.Variants, src => src.Variants)
            .Map(dest => dest.Specifications, src => src.Specifications)
            .Map(dest => dest.Images, src => src.Images)
            .Map(dest => dest.Translations, src => src.Translations)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.ModifiedAt, src => src.ModifiedAt)
            .Map(dest => dest.IsActive, src => !src.IsDeleted);

        // Product -> ProductListDto
        config.NewConfig<Product, ProductListDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Sku, src => src.Sku.Value)
            .Map(dest => dest.Slug, src => src.Slug.Value)
            .Map(dest => dest.Price, src => src.Price.Amount)
            .Map(dest => dest.Currency, src => src.Price.Currency)
            .Map(dest => dest.MainImageUrl, src => GetMainImageUrl(src.Images))
            .Map(dest => dest.CategoryName, src => src.Category != null ? src.Category.Name : null)
            .Map(dest => dest.CategorySlug, src => src.Category != null ? src.Category.Slug.Value : null)
            .Map(dest => dest.BrandName, src => src.Brand != null ? src.Brand.Name : null)
            .Map(dest => dest.BrandSlug, src => src.Brand != null ? src.Brand.Slug.Value : null)
            .Map(dest => dest.HasVariants, src => src.Variants.Any())
            .Map(dest => dest.TotalStock, src => src.Variants.Any() 
                ? src.Variants.Sum(v => v.StockQuantity) 
                : -1) // -1 means unlimited stock (no variants)
            .Map(dest => dest.IsActive, src => !src.IsDeleted);

        // ProductVariant -> ProductVariantDto
        config.NewConfig<ProductVariant, ProductVariantDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Sku, src => src.Sku.Value)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.PriceAdjustmentAmount, src => src.PriceAdjustmentAmount)
            .Map(dest => dest.PriceAdjustmentCurrency, src => src.PriceAdjustmentCurrency)
            .Map(dest => dest.StockQuantity, src => src.StockQuantity);

        // ProductSpecification -> ProductSpecificationDto
        config.NewConfig<ProductSpecification, ProductSpecificationDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Value, src => src.Value);

        // ProductImage -> ProductImageDto
        config.NewConfig<ProductImage, ProductImageDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Url, src => src.Url)
            .Map(dest => dest.AltText, src => src.AltText)
            .Map(dest => dest.SortOrder, src => src.SortOrder)
            .Map(dest => dest.IsMain, src => src.IsMain);

        // ProductTranslation -> ProductTranslationDto
        config.NewConfig<ProductTranslation, ProductTranslationDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.LanguageCode, src => src.LanguageCode)
            .Map(dest => dest.Title, src => src.Title)
            .Map(dest => dest.Description, src => src.Description);
    }

    /// <summary>
    /// Gets the main image URL from a collection of product images.
    /// Returns the main image if available, otherwise the first image by sort order.
    /// </summary>
    private static string? GetMainImageUrl(IEnumerable<ProductImage>? images)
    {
        if (images == null)
        {
            // Debug: images collection is null
            System.Diagnostics.Debug.WriteLine("GetMainImageUrl: images is null");
            return null;
        }

        var imageList = images.ToList();
        if (!imageList.Any())
        {
            // Debug: no images in collection
            System.Diagnostics.Debug.WriteLine("GetMainImageUrl: no images in collection");
            return null;
        }

        // Debug: found images
        System.Diagnostics.Debug.WriteLine($"GetMainImageUrl: found {imageList.Count} images");

        // First try to get the main image
        var mainImage = imageList.FirstOrDefault(i => i.IsMain);
        if (mainImage != null && !string.IsNullOrWhiteSpace(mainImage.Url))
        {
            System.Diagnostics.Debug.WriteLine($"GetMainImageUrl: using main image: {mainImage.Url}");
            return mainImage.Url;
        }

        // If no main image, get the first image sorted by SortOrder
        var firstImage = imageList.OrderBy(i => i.SortOrder).FirstOrDefault();
        if (firstImage != null && !string.IsNullOrWhiteSpace(firstImage.Url))
        {
            System.Diagnostics.Debug.WriteLine($"GetMainImageUrl: using first image: {firstImage.Url}");
            return firstImage.Url;
        }

        System.Diagnostics.Debug.WriteLine("GetMainImageUrl: no valid image URL found");
        return null;
    }
}
