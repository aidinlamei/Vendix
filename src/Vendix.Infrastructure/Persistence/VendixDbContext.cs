using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Vendix.Domain.Catalog.Entities;

namespace Vendix.Infrastructure.Persistence;

/// <summary>
/// The main database context for the Vendix application.
/// </summary>
/// <remarks>
/// This context is responsible for all database operations and entity tracking.
/// It applies configurations from the Configurations folder and handles
/// soft delete filtering automatically.
/// </remarks>
public class VendixDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VendixDbContext"/> class.
    /// </summary>
    /// <param name="options">The options to be used by the context.</param>
    public VendixDbContext(DbContextOptions<VendixDbContext> options) : base(options)
    {
    }

    #region Catalog

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

    /// <summary>
    /// Configures the model using fluent API.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from the current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
