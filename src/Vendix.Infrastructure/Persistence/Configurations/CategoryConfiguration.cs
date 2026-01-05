using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vendix.Domain.Catalog.Entities;
using Vendix.Domain.Catalog.ValueObjects;

namespace Vendix.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="Category"/> aggregate root.
/// </summary>
public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        // Primary Key
        builder.HasKey(c => c.Id);

        // Name
        builder.Property(c => c.Name)
            .HasMaxLength(200)
            .IsRequired();

        // Slug Value Object Conversion
        builder.Property(c => c.Slug)
            .HasMaxLength(Slug.MaxLength)
            .IsRequired()
            .HasConversion(
                slug => slug.Value,
                value => new Slug(value));

        builder.HasIndex(c => c.Slug)
            .IsUnique();

        // Parent Category Self-Reference
        builder.HasOne(c => c.ParentCategory)
            .WithMany(c => c.SubCategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        // Audit Fields
        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.CreatedBy)
            .HasMaxLength(100);

        builder.Property(c => c.ModifiedBy)
            .HasMaxLength(100);

        // Soft Delete
        builder.Property(c => c.IsDeleted)
            .HasDefaultValue(false);

        builder.Property(c => c.DeletedBy)
            .HasMaxLength(100);

        builder.HasQueryFilter(c => !c.IsDeleted);

        // Concurrency Token - PostgreSQL uses bytea for row version
        builder.Property(c => c.RowVersion)
            .HasColumnType("bytea")
            .IsConcurrencyToken()
            .HasDefaultValueSql("'\\x0000000000000000'::bytea");

        // Translations Collection
        builder.HasMany(c => c.Translations)
            .WithOne(t => t.Category)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure backing fields for collections
        builder.Navigation(c => c.Products)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(c => c.Translations)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(c => c.SubCategories)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

/// <summary>
/// EF Core configuration for the <see cref="CategoryTranslation"/> entity.
/// </summary>
public class CategoryTranslationConfiguration : IEntityTypeConfiguration<CategoryTranslation>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CategoryTranslation> builder)
    {
        builder.ToTable("CategoryTranslations");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.LanguageCode)
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(t => t.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasMaxLength(2000);

        // Unique index for category + language
        builder.HasIndex(t => new { t.CategoryId, t.LanguageCode })
            .IsUnique();
    }
}
