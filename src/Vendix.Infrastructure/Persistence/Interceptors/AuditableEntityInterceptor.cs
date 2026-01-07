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

        // RowVersion update logic disabled - concurrency token removed for PostgreSQL compatibility
        // Update RowVersion for AggregateRoot entities (PostgreSQL doesn't auto-update bytea)
        // foreach (var entry in context.ChangeTracker.Entries<AggregateRoot>())
        // {
        //     if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
        //     {
        //         var originalRowVersion = entry.Property(nameof(AggregateRoot.RowVersion)).OriginalValue as byte[];
        //         var currentRowVersion = entry.Entity.RowVersion;
        //         
        //         // Generate a new RowVersion using current timestamp and random bytes
        //         var timestamp = BitConverter.GetBytes(utcNow.Ticks);
        //         var random = Guid.NewGuid().ToByteArray();
        //         var newRowVersion = new byte[timestamp.Length + random.Length];
        //         Buffer.BlockCopy(timestamp, 0, newRowVersion, 0, timestamp.Length);
        //         Buffer.BlockCopy(random, 0, newRowVersion, timestamp.Length, random.Length);
        //         
        //         // Log for debugging
        //         System.Diagnostics.Debug.WriteLine($"[RowVersion Debug] Entity: {entry.Entity.GetType().Name}, Id: {entry.Entity.Id}");
        //         System.Diagnostics.Debug.WriteLine($"[RowVersion Debug] State: {entry.State}");
        //         System.Diagnostics.Debug.WriteLine($"[RowVersion Debug] Original RowVersion: {(originalRowVersion != null ? Convert.ToHexString(originalRowVersion) : "null")}");
        //         System.Diagnostics.Debug.WriteLine($"[RowVersion Debug] Current RowVersion: {(currentRowVersion != null ? Convert.ToHexString(currentRowVersion) : "null")}");
        //         System.Diagnostics.Debug.WriteLine($"[RowVersion Debug] New RowVersion: {Convert.ToHexString(newRowVersion)}");
        //         
        //         // Only update the entity property, NOT OriginalValues
        //         // EF Core will use OriginalValues in WHERE clause and new value in SET clause
        //         entry.Entity.RowVersion = newRowVersion;
        //         
        //         // Mark RowVersion as modified so EF Core includes it in UPDATE SET
        //         entry.Property(nameof(AggregateRoot.RowVersion)).IsModified = true;
        //         
        //         var finalOriginal = entry.Property(nameof(AggregateRoot.RowVersion)).OriginalValue as byte[];
        //         var finalCurrent = entry.Entity.RowVersion;
        //         System.Diagnostics.Debug.WriteLine($"[RowVersion Debug] After update - Original: {(finalOriginal != null ? Convert.ToHexString(finalOriginal) : "null")}, Current: {(finalCurrent != null ? Convert.ToHexString(finalCurrent) : "null")}");
        //     }
        // }
    }
}
