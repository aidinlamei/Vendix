using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vendix.Domain.Catalog.Entities;
using Vendix.Domain.Catalog.ValueObjects;

namespace Vendix.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the Brand entity.
/// </summary>
public class BrandConfiguration : IEntityTypeConfiguration<Brand>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.ToTable("Brands");

        // Primary key
        builder.HasKey(b => b.Id);

        // Properties
        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(100);

        // Value Object: Slug
        builder.Property(b => b.Slug)
            .IsRequired()
            .HasMaxLength(Slug.MaxLength)
            .HasConversion(
                slug => slug.Value,
                value => new Slug(value));

        builder.Property(b => b.LogoUrl)
            .HasMaxLength(2000);

        // Indexes
        builder.HasIndex(b => b.Slug)
            .IsUnique()
            .HasDatabaseName("IX_Brands_Slug");

        builder.HasIndex(b => b.Name)
            .HasDatabaseName("IX_Brands_Name");
    }
}
