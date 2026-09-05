using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vendix.Domain.Basket.Entities;

namespace Vendix.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="Basket"/> aggregate root.
/// </summary>
public class BasketConfiguration : IEntityTypeConfiguration<Basket>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Basket> builder)
    {
        builder.ToTable("Baskets");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.BuyerId)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(b => b.BuyerId)
            .IsUnique();

        builder.HasMany(b => b.Items)
            .WithOne()
            .HasForeignKey(i => i.BasketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(b => b.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

/// <summary>
/// EF Core configuration for the <see cref="BasketItem"/> entity.
/// </summary>
public class BasketItemConfiguration : IEntityTypeConfiguration<BasketItem>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<BasketItem> builder)
    {
        builder.ToTable("BasketItems");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.ProductName).HasMaxLength(200).IsRequired();
        builder.Property(i => i.ProductSlug).HasMaxLength(200).IsRequired();
        builder.Property(i => i.Sku).HasMaxLength(50).IsRequired();
        builder.Property(i => i.ImageUrl).HasMaxLength(1000);
        builder.Property(i => i.Quantity).IsRequired();

        builder.OwnsOne(i => i.UnitPrice, priceBuilder =>
        {
            priceBuilder.Property(m => m.Amount)
                .HasColumnName("UnitPrice")
                .HasPrecision(18, 4)
                .IsRequired();

            priceBuilder.Property(m => m.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.HasIndex(i => new { i.BasketId, i.ProductId })
            .IsUnique();
    }
}
