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
    private readonly ICurrentUserService? _currentUserService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditableEntityInterceptor"/> class.
    /// </summary>
    /// <param name="dateTimeProvider">The date time provider for getting current UTC time.</param>
    /// <param name="currentUserService">The current user service for getting user information (optional).</param>
    public AuditableEntityInterceptor(
        IDateTimeProvider dateTimeProvider,
        ICurrentUserService? currentUserService = null)
    {
        _dateTimeProvider = dateTimeProvider;
        _currentUserService = currentUserService;
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
        var userId = _currentUserService?.UserId;

        foreach (var entry in context.ChangeTracker.Entries<IAuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = utcNow;
                    entry.Entity.CreatedBy = userId;
                    break;

                case EntityState.Modified:
                    entry.Entity.ModifiedAt = utcNow;
                    entry.Entity.ModifiedBy = userId;
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
                    entry.Entity.DeletedBy = userId;
                }
            }
        }
    }
}
