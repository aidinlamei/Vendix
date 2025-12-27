namespace Vendix.Domain.Common;

/// <summary>
/// Interface for entities that support audit trail tracking.
/// </summary>
/// <remarks>
/// Implementing this interface enables automatic population of audit fields
/// via EF Core interceptors when entities are created or modified.
/// </remarks>
public interface IAuditableEntity
{
    /// <summary>
    /// Gets or sets the date and time when this entity was created.
    /// </summary>
    DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who created this entity.
    /// </summary>
    /// <remarks>
    /// This may be null for system-generated entities or when created anonymously.
    /// </remarks>
    string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this entity was last modified.
    /// </summary>
    /// <remarks>
    /// This is null if the entity has never been modified after creation.
    /// </remarks>
    DateTime? ModifiedAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who last modified this entity.
    /// </summary>
    /// <remarks>
    /// This is null if the entity has never been modified after creation.
    /// </remarks>
    string? ModifiedBy { get; set; }
}
