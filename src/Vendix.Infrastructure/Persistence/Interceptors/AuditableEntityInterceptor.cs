using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Vendix.Application.Common.Interfaces;
using Vendix.Domain.Common;

namespace Vendix.Infrastructure.Persistence.Interceptors;

/// <summary>
/// EF Core interceptor that automatically sets audit fields on entities.
/// </summary>
/// <remarks>
/// This interceptor handles:
/// - Setting CreatedAt and CreatedBy on entity creation
/// - Setting ModifiedAt and ModifiedBy on entity update
/// - Setting DeletedAt and DeletedBy on soft delete
/// </remarks>
public sealed class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly IDateTimeProvider _dateTimeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditableEntityInterceptor"/> class.
    /// </summary>
    /// <param name="dateTimeProvider">The date time provider.</param>
    public AuditableEntityInterceptor(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateEntities(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var utcNow = _dateTimeProvider.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries<IAuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    SetCreatedProperties(entry, utcNow);
                    break;

                case EntityState.Modified:
                    SetModifiedProperties(entry, utcNow);
                    break;
            }
        }

        // Handle soft delete
        foreach (var entry in context.ChangeTracker.Entries<ISoftDelete>())
        {
            if (entry.State == EntityState.Modified && entry.Entity.IsDeleted)
            {
                // Check if IsDeleted was just changed to true
                var originalIsDeleted = entry.OriginalValues.GetValue<bool>(nameof(ISoftDelete.IsDeleted));
                if (!originalIsDeleted && entry.Entity.IsDeleted)
                {
                    SetDeletedProperties(entry, utcNow);
                }
            }
        }
    }

    private static void SetCreatedProperties(EntityEntry<IAuditableEntity> entry, DateTime utcNow)
    {
        entry.Entity.CreatedAt = utcNow;
        // CreatedBy would typically come from ICurrentUser service
        // For now, we leave it as null or let it be set by the application
    }

    private static void SetModifiedProperties(EntityEntry<IAuditableEntity> entry, DateTime utcNow)
    {
        entry.Entity.ModifiedAt = utcNow;
        // ModifiedBy would typically come from ICurrentUser service
    }

    private static void SetDeletedProperties(EntityEntry<ISoftDelete> entry, DateTime utcNow)
    {
        entry.Entity.DeletedAt = utcNow;
        // DeletedBy would typically come from ICurrentUser service
    }
}
