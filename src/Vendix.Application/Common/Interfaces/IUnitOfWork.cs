namespace Vendix.Application.Common.Interfaces;

/// <summary>
/// Represents a unit of work for coordinating changes across multiple repositories.
/// </summary>
/// <remarks>
/// The unit of work pattern ensures that all changes made within a business transaction
/// are committed atomically. If any operation fails, all changes can be rolled back.
/// </remarks>
public interface IUnitOfWork
{
    /// <summary>
    /// Persists all changes made within this unit of work to the database.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The number of entities written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
