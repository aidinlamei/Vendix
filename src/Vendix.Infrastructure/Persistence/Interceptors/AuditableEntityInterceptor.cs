using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Vendix.Application.Common.Interfaces;
using Vendix.Domain.Common;

namespace Vendix.Infrastructure.Persistence.Interceptors;

/// <summary>
/// EF Core interceptor that automatically sets audit fields on entities implementing <see cref="IAuditableEntity"/>.
/// </summary>
/// <remarks>
/// This interceptor is called before SaveChanges and SaveChangesAsync to populate:
/// - CreatedAt and CreatedBy for new entities
/// - ModifiedAt and ModifiedBy for modified entities
/// - DeletedAt and DeletedBy for soft-deleted entities (if implementing <see cref="ISoftDelete"/>)
/// </remarks>
public sealed class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly IDateTimeProvider _dateTimeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditableEntityInterceptor"/> class.
    /// </summary>
    /// <param name="dateTimeProvider">The date time provider for getting current UTC time.</param>
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
                    entry.Entity.CreatedAt = utcNow;
                    // CreatedBy would be set here if we had ICurrentUserService
                    // entry.Entity.CreatedBy = _currentUserService.UserId;
                    break;

                case EntityState.Modified:
                    entry.Entity.ModifiedAt = utcNow;
                    // ModifiedBy would be set here if we had ICurrentUserService
                    // entry.Entity.ModifiedBy = _currentUserService.UserId;
                    break;
            }
        }

        foreach (var entry in context.ChangeTracker.Entries<ISoftDelete>())
        {
            if (entry.State == EntityState.Modified && entry.Entity.IsDeleted)
            {
                // Check if IsDeleted was just set to true
                var originalIsDeleted = entry.OriginalValues.GetValue<bool>(nameof(ISoftDelete.IsDeleted));
                if (!originalIsDeleted)
                {
                    entry.Entity.DeletedAt = utcNow;
                    // DeletedBy would be set here if we had ICurrentUserService
                    // entry.Entity.DeletedBy = _currentUserService.UserId;
                }
            }
        }
    }
}
