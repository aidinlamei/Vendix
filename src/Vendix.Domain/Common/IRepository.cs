namespace Vendix.Domain.Common;

/// <summary>
/// Generic repository interface for aggregate roots.
/// </summary>
/// <typeparam name="T">The type of aggregate root this repository manages.</typeparam>
/// <remarks>
/// Repositories provide a collection-like interface for accessing domain objects.
/// They encapsulate the logic for retrieving and storing aggregates, hiding the
/// underlying persistence mechanism from the domain layer.
/// Only aggregate roots should have repositories.
/// </remarks>
public interface IRepository<T> where T : AggregateRoot
{
    /// <summary>
    /// Retrieves an aggregate by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the aggregate to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The aggregate if found; otherwise, null.</returns>
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new aggregate to the repository.
    /// </summary>
    /// <param name="entity">The aggregate to add.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when entity is null.</exception>
    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing aggregate in the repository.
    /// </summary>
    /// <param name="entity">The aggregate to update.</param>
    /// <remarks>
    /// The aggregate must already exist in the repository.
    /// Changes are tracked and persisted when SaveChangesAsync is called on the unit of work.
    /// </remarks>
    void Update(T entity);

    /// <summary>
    /// Removes an aggregate from the repository.
    /// </summary>
    /// <param name="entity">The aggregate to remove.</param>
    /// <remarks>
    /// For entities implementing <see cref="ISoftDelete"/>, consider using soft deletion instead.
    /// </remarks>
    void Delete(T entity);
}
