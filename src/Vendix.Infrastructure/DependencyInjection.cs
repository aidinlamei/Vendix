using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Vendix.Application.Common.Interfaces;
using Vendix.Domain.Catalog.Repositories;
using Vendix.Infrastructure.Persistence;
using Vendix.Infrastructure.Persistence.Interceptors;
using Vendix.Infrastructure.Persistence.Repositories;
using Vendix.Infrastructure.Services;

namespace Vendix.Infrastructure;

/// <summary>
/// Extension methods for registering infrastructure services with the dependency injection container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds infrastructure services to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="connectionString">The PostgreSQL connection string.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when connectionString is null or empty.</exception>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        // Register services
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        // Register interceptors
        services.AddScoped<AuditableEntityInterceptor>();

        // Register DbContext
        services.AddDbContext<VendixDbContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(VendixDbContext).Assembly.FullName);
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
            });

            // Add interceptors
            options.AddInterceptors(
                serviceProvider.GetRequiredService<AuditableEntityInterceptor>());

#if DEBUG
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
#endif
        });

        // Register UnitOfWork
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register Repositories
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IBrandRepository, BrandRepository>();

        return services;
    }
}
