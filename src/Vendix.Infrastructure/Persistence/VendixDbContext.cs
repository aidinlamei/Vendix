using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Vendix.Domain.Catalog.Entities;

namespace Vendix.Infrastructure.Persistence;

/// <summary>
/// The main database context for the Vendix application.
/// </summary>
/// <remarks>
/// This context manages all entity sets and applies configurations from the current assembly.
/// It supports soft delete through global query filters and audit tracking via interceptors.
/// </remarks>
public class VendixDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VendixDbContext"/> class.
    /// </summary>
    /// <param name="options">The options to configure the context.</param>
    public VendixDbContext(DbContextOptions<VendixDbContext> options) : base(options)
    {
    }

    #region Catalog Entities

    /// <summary>
    /// Gets or sets the products DbSet.
    /// </summary>
    public DbSet<Product> Products => Set<Product>();

    /// <summary>
    /// Gets or sets the categories DbSet.
    /// </summary>
    public DbSet<Category> Categories => Set<Category>();

    /// <summary>
    /// Gets or sets the brands DbSet.
    /// </summary>
    public DbSet<Brand> Brands => Set<Brand>();

    /// <summary>
    /// Gets or sets the product variants DbSet.
    /// </summary>
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();

    /// <summary>
    /// Gets or sets the product specifications DbSet.
    /// </summary>
    public DbSet<ProductSpecification> ProductSpecifications => Set<ProductSpecification>();

    /// <summary>
    /// Gets or sets the product images DbSet.
    /// </summary>
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();

    /// <summary>
    /// Gets or sets the product translations DbSet.
    /// </summary>
    public DbSet<ProductTranslation> ProductTranslations => Set<ProductTranslation>();

    /// <summary>
    /// Gets or sets the category translations DbSet.
    /// </summary>
    public DbSet<CategoryTranslation> CategoryTranslations => Set<CategoryTranslation>();

    #endregion

    #region Basket Entities

    /// <summary>
    /// Gets or sets the baskets DbSet.
    /// </summary>
    public DbSet<Vendix.Domain.Basket.Entities.Basket> Baskets => Set<Vendix.Domain.Basket.Entities.Basket>();

    /// <summary>
    /// Gets or sets the basket items DbSet.
    /// </summary>
    public DbSet<Vendix.Domain.Basket.Entities.BasketItem> BasketItems => Set<Vendix.Domain.Basket.Entities.BasketItem>();

    #endregion

    #region Ordering Entities

    /// <summary>
    /// Gets or sets the orders DbSet.
    /// </summary>
    public DbSet<Vendix.Domain.Ordering.Entities.Order> Orders => Set<Vendix.Domain.Ordering.Entities.Order>();

    /// <summary>
    /// Gets or sets the order items DbSet.
    /// </summary>
    public DbSet<Vendix.Domain.Ordering.Entities.OrderItem> OrderItems => Set<Vendix.Domain.Ordering.Entities.OrderItem>();

    #endregion

    /// <summary>
    /// Configures the model that was discovered by convention from the entity types
    /// exposed in <see cref="DbSet{TEntity}"/> properties on your derived context.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from the current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
