namespace Vendix.Domain.Common;

/// <summary>
/// Interface for entities that support soft deletion.
/// </summary>
/// <remarks>
/// Soft deletion marks entities as deleted without physically removing them from the database.
/// This enables data recovery, audit trails, and referential integrity preservation.
/// Query filters should be applied to exclude soft-deleted entities from normal queries.
/// </remarks>
public interface ISoftDelete
{
    /// <summary>
    /// Gets or sets a value indicating whether this entity has been soft deleted.
    /// </summary>
    bool IsDeleted { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this entity was soft deleted.
    /// </summary>
    /// <remarks>
    /// This is null if the entity has not been deleted.
    /// </remarks>
    DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who deleted this entity.
    /// </summary>
    /// <remarks>
    /// This is null if the entity has not been deleted.
    /// </remarks>
    string? DeletedBy { get; set; }

    /// <summary>
    /// Marks this entity as deleted.
    /// </summary>
    /// <param name="deletedBy">Optional identifier of the user who deleted this entity.</param>
    /// <remarks>
    /// Sets the IsDeleted flag to true and records the deletion timestamp.
    /// If deletedBy is provided, it will be set; otherwise, it may be set automatically by the soft delete interceptor.
    /// </remarks>
    void MarkAsDeleted(string? deletedBy = null);
}
