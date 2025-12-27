using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vendix.Domain.Catalog.Entities;
using Vendix.Domain.Catalog.ValueObjects;

namespace Vendix.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the Product aggregate root.
/// </summary>
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        // Primary key
        builder.HasKey(p => p.Id);

        // Properties
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(4000);

        // Value Object: Sku
        builder.Property(p => p.Sku)
            .IsRequired()
            .HasMaxLength(Sku.MaxLength)
            .HasConversion(
                sku => sku.Value,
                value => new Sku(value));

        // Value Object: Slug
        builder.Property(p => p.Slug)
            .IsRequired()
            .HasMaxLength(Slug.MaxLength)
            .HasConversion(
                slug => slug.Value,
                value => new Slug(value));

        // Value Object: Money (stored as two columns)
        builder.OwnsOne(p => p.Price, priceBuilder =>
        {
            priceBuilder.Property(m => m.Amount)
                .HasColumnName("Price")
                .HasPrecision(18, 2)
                .IsRequired();

            priceBuilder.Property(m => m.Currency)
                .HasColumnName("PriceCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.Property(p => p.ProductType)
            .IsRequired();

        // Indexes
        builder.HasIndex(p => p.Slug)
            .IsUnique()
            .HasDatabaseName("IX_Products_Slug");

        builder.HasIndex(p => p.Sku)
            .IsUnique()
            .HasDatabaseName("IX_Products_Sku");

        builder.HasIndex(p => p.CategoryId)
            .HasDatabaseName("IX_Products_CategoryId");

        builder.HasIndex(p => p.BrandId)
            .HasDatabaseName("IX_Products_BrandId");

        // Soft delete filter
        builder.HasQueryFilter(p => !p.IsDeleted);

        // Relationships
        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.Brand)
            .WithMany()
            .HasForeignKey(p => p.BrandId)
            .OnDelete(DeleteBehavior.SetNull);

        // Child entities
        builder.HasMany(p => p.Variants)
            .WithOne(v => v.Product)
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Specifications)
            .WithOne(s => s.Product)
            .HasForeignKey(s => s.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Images)
            .WithOne(i => i.Product)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Translations)
            .WithOne(t => t.Product)
            .HasForeignKey(t => t.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events
        builder.Ignore(p => p.DomainEvents);
    }
}

/// <summary>
/// EF Core configuration for the ProductVariant entity.
/// </summary>
public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("ProductVariants");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Name)
            .IsRequired()
            .HasMaxLength(100);

        // Value Object: Sku
        builder.Property(v => v.Sku)
            .IsRequired()
            .HasMaxLength(Sku.MaxLength)
            .HasConversion(
                sku => sku.Value,
                value => new Sku(value));

        // Value Object: Money (PriceAdjustment)
        builder.OwnsOne(v => v.PriceAdjustment, priceBuilder =>
        {
            priceBuilder.Property(m => m.Amount)
                .HasColumnName("PriceAdjustment")
                .HasPrecision(18, 2)
                .IsRequired();

            priceBuilder.Property(m => m.Currency)
                .HasColumnName("PriceAdjustmentCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.Property(v => v.StockQuantity)
            .IsRequired();

        builder.HasIndex(v => v.Sku)
            .IsUnique()
            .HasDatabaseName("IX_ProductVariants_Sku");
    }
}

/// <summary>
/// EF Core configuration for the ProductSpecification entity.
/// </summary>
public class ProductSpecificationConfiguration : IEntityTypeConfiguration<ProductSpecification>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ProductSpecification> builder)
    {
        builder.ToTable("ProductSpecifications");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Value)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasIndex(s => new { s.ProductId, s.Name })
            .IsUnique()
            .HasDatabaseName("IX_ProductSpecifications_ProductId_Name");
    }
}

/// <summary>
/// EF Core configuration for the ProductImage entity.
/// </summary>
public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("ProductImages");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Url)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(i => i.AltText)
            .HasMaxLength(500);

        builder.Property(i => i.SortOrder)
            .IsRequired();

        builder.Property(i => i.IsMain)
            .IsRequired();

        builder.HasIndex(i => new { i.ProductId, i.SortOrder })
            .HasDatabaseName("IX_ProductImages_ProductId_SortOrder");
    }
}

/// <summary>
/// EF Core configuration for the ProductTranslation entity.
/// </summary>
public class ProductTranslationConfiguration : IEntityTypeConfiguration<ProductTranslation>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ProductTranslation> builder)
    {
        builder.ToTable("ProductTranslations");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.LanguageCode)
            .IsRequired()
            .HasMaxLength(2);

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .HasMaxLength(4000);

        builder.HasIndex(t => new { t.ProductId, t.LanguageCode })
            .IsUnique()
            .HasDatabaseName("IX_ProductTranslations_ProductId_LanguageCode");
    }
}
