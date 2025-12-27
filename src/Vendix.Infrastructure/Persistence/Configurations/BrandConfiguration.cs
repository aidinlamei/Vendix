using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vendix.Domain.Catalog.Entities;
using Vendix.Domain.Catalog.ValueObjects;

namespace Vendix.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="Brand"/> entity.
/// </summary>
public class BrandConfiguration : IEntityTypeConfiguration<Brand>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.ToTable("Brands");

        // Primary Key
        builder.HasKey(b => b.Id);

        // Name
        builder.Property(b => b.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(b => b.Name)
            .IsUnique();

        // Slug Value Object Conversion
        builder.Property(b => b.Slug)
            .HasMaxLength(Slug.MaxLength)
            .IsRequired()
            .HasConversion(
                slug => slug.Value,
                value => new Slug(value));

        builder.HasIndex(b => b.Slug)
            .IsUnique();

        // Logo URL
        builder.Property(b => b.LogoUrl)
            .HasMaxLength(1000);

        // Audit Fields
        builder.Property(b => b.CreatedAt)
            .IsRequired();

        builder.Property(b => b.CreatedBy)
            .HasMaxLength(100);

        builder.Property(b => b.ModifiedBy)
            .HasMaxLength(100);

        // Soft Delete
        builder.Property(b => b.IsDeleted)
            .HasDefaultValue(false);

        builder.Property(b => b.DeletedBy)
            .HasMaxLength(100);

        builder.HasQueryFilter(b => !b.IsDeleted);
    }
}
