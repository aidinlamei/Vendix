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
            .Map(dest => dest.BrandId, src => src.BrandId)
            .Map(dest => dest.BrandName, src => src.Brand != null ? src.Brand.Name : null)
            .Map(dest => dest.MainImageUrl, src => src.Images
                .Where(i => i.IsMain)
                .Select(i => i.Url)
                .FirstOrDefault())
            .Map(dest => dest.Variants, src => src.Variants)
            .Map(dest => dest.Specifications, src => src.Specifications)
            .Map(dest => dest.Images, src => src.Images)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.ModifiedAt, src => src.ModifiedAt);

        // Product -> ProductListDto
        config.NewConfig<Product, ProductListDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Slug, src => src.Slug.Value)
            .Map(dest => dest.Price, src => src.Price.Amount)
            .Map(dest => dest.Currency, src => src.Price.Currency)
            .Map(dest => dest.MainImageUrl, src => src.Images
                .Where(i => i.IsMain)
                .Select(i => i.Url)
                .FirstOrDefault())
            .Map(dest => dest.CategoryName, src => src.Category != null ? src.Category.Name : null)
            .Map(dest => dest.BrandName, src => src.Brand != null ? src.Brand.Name : null);

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
    }
}
