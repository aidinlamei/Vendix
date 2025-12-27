using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vendix.Domain.Catalog.Entities;
using Vendix.Domain.Catalog.ValueObjects;

namespace Vendix.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="Product"/> aggregate root.
/// </summary>
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        // Primary Key
        builder.HasKey(p => p.Id);

        // Name
        builder.Property(p => p.Name)
            .HasMaxLength(200)
            .IsRequired();

        // Description
        builder.Property(p => p.Description)
            .HasMaxLength(4000);

        // SKU Value Object Conversion
        builder.Property(p => p.Sku)
            .HasMaxLength(Sku.MaxLength)
            .IsRequired()
            .HasConversion(
                sku => sku.Value,
                value => new Sku(value));

        builder.HasIndex(p => p.Sku)
            .IsUnique();

        // Slug Value Object Conversion
        builder.Property(p => p.Slug)
            .HasMaxLength(Slug.MaxLength)
            .IsRequired()
            .HasConversion(
                slug => slug.Value,
                value => new Slug(value));

        builder.HasIndex(p => p.Slug)
            .IsUnique();

        // Money Value Object - Owned Entity
        builder.OwnsOne(p => p.Price, priceBuilder =>
        {
            priceBuilder.Property(m => m.Amount)
                .HasColumnName("Price")
                .HasPrecision(18, 4)
                .IsRequired();

            priceBuilder.Property(m => m.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // ProductType Enum
        builder.Property(p => p.ProductType)
            .IsRequired();

        // Category Relationship (optional)
        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        // Brand Relationship (optional)
        builder.HasOne(p => p.Brand)
            .WithMany()
            .HasForeignKey(p => p.BrandId)
            .OnDelete(DeleteBehavior.SetNull);

        // Audit Fields
        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.CreatedBy)
            .HasMaxLength(100);

        builder.Property(p => p.ModifiedBy)
            .HasMaxLength(100);

        // Soft Delete
        builder.Property(p => p.IsDeleted)
            .HasDefaultValue(false);

        builder.Property(p => p.DeletedBy)
            .HasMaxLength(100);

        builder.HasQueryFilter(p => !p.IsDeleted);

        // Concurrency Token
        builder.Property(p => p.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // Variants Collection
        builder.HasMany(p => p.Variants)
            .WithOne(v => v.Product)
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Specifications Collection
        builder.HasMany(p => p.Specifications)
            .WithOne(s => s.Product)
            .HasForeignKey(s => s.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Images Collection
        builder.HasMany(p => p.Images)
            .WithOne(i => i.Product)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Translations Collection
        builder.HasMany(p => p.Translations)
            .WithOne(t => t.Product)
            .HasForeignKey(t => t.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure backing fields for collections
        builder.Navigation(p => p.Variants)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(p => p.Specifications)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(p => p.Images)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(p => p.Translations)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

/// <summary>
/// EF Core configuration for the <see cref="ProductVariant"/> entity.
/// </summary>
public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("ProductVariants");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Name)
            .HasMaxLength(200)
            .IsRequired();

        // SKU Value Object Conversion
        builder.Property(v => v.Sku)
            .HasMaxLength(Sku.MaxLength)
            .IsRequired()
            .HasConversion(
                sku => sku.Value,
                value => new Sku(value));

        builder.HasIndex(v => v.Sku)
            .IsUnique();

        // Price Adjustment (can be negative for discounts)
        builder.Property(v => v.PriceAdjustmentAmount)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(v => v.PriceAdjustmentCurrency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(v => v.StockQuantity)
            .IsRequired();
    }
}

/// <summary>
/// EF Core configuration for the <see cref="ProductSpecification"/> entity.
/// </summary>
public class ProductSpecificationConfiguration : IEntityTypeConfiguration<ProductSpecification>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ProductSpecification> builder)
    {
        builder.ToTable("ProductSpecifications");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.Value)
            .HasMaxLength(500)
            .IsRequired();

        // Index for faster lookups by product and name
        builder.HasIndex(s => new { s.ProductId, s.Name });
    }
}

/// <summary>
/// EF Core configuration for the <see cref="ProductImage"/> entity.
/// </summary>
public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("ProductImages");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Url)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(i => i.AltText)
            .HasMaxLength(500);

        builder.Property(i => i.SortOrder)
            .IsRequired();

        builder.Property(i => i.IsMain)
            .HasDefaultValue(false);

        // Index for main image lookup
        builder.HasIndex(i => new { i.ProductId, i.IsMain });
    }
}

/// <summary>
/// EF Core configuration for the <see cref="ProductTranslation"/> entity.
/// </summary>
public class ProductTranslationConfiguration : IEntityTypeConfiguration<ProductTranslation>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ProductTranslation> builder)
    {
        builder.ToTable("ProductTranslations");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.LanguageCode)
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(t => t.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasMaxLength(4000);

        // Unique index for product + language
        builder.HasIndex(t => new { t.ProductId, t.LanguageCode })
            .IsUnique();
    }
}
