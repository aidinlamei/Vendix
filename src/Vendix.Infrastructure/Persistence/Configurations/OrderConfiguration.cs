using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vendix.Domain.Ordering.Entities;
using Vendix.Domain.Ordering.ValueObjects;

namespace Vendix.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="Order"/> aggregate root.
/// </summary>
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderNumber)
            .HasMaxLength(OrderNumber.Length)
            .IsRequired()
            .HasConversion(
                number => number.Value,
                value => new OrderNumber(value));

        builder.HasIndex(o => o.OrderNumber)
            .IsUnique();

        builder.Property(o => o.BuyerId).HasMaxLength(100).IsRequired();
        builder.Property(o => o.BuyerEmail).HasMaxLength(256).IsRequired();
        builder.Property(o => o.ShippingAddress).HasMaxLength(500).IsRequired();
        builder.Property(o => o.Currency).HasMaxLength(3).IsRequired();
        builder.Property(o => o.ShippingCost).HasPrecision(18, 4).IsRequired();
        builder.Property(o => o.Status).IsRequired();

        builder.Property(o => o.CreatedAt).IsRequired();
        builder.Property(o => o.CreatedBy).HasMaxLength(100);
        builder.Property(o => o.ModifiedBy).HasMaxLength(100);

        builder.HasIndex(o => o.BuyerId);

        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(o => o.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

/// <summary>
/// EF Core configuration for the <see cref="OrderItem"/> entity.
/// </summary>
public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.ProductName).HasMaxLength(200).IsRequired();
        builder.Property(i => i.Sku).HasMaxLength(50).IsRequired();
        builder.Property(i => i.UnitPrice).HasPrecision(18, 4).IsRequired();
        builder.Property(i => i.ImageUrl).HasMaxLength(1000);
        builder.Property(i => i.Quantity).IsRequired();
    }
}
