namespace Vendix.Application.Common.Interfaces;

/// <summary>
/// Represents the unit of work pattern for coordinating changes across multiple repositories.
/// </summary>
/// <remarks>
/// The unit of work maintains a list of objects affected by a business transaction
/// and coordinates the writing out of changes and the resolution of concurrency problems.
/// </remarks>
public interface IUnitOfWork
{
    /// <summary>
    /// Saves all changes made in this unit of work to the underlying database.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A task that represents the asynchronous save operation.
    /// The task result contains the number of state entries written to the database.
    /// </returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
