namespace Vendix.Domain.Common;

/// <summary>
/// Base class for all domain entities providing identity and equality comparison.
/// </summary>
/// <remarks>
/// Entities are equal when they have the same identity (Id), regardless of other property values.
/// This follows DDD principles where entity identity is what distinguishes one entity from another.
/// </remarks>
public abstract class BaseEntity : IEquatable<BaseEntity>
{
    /// <summary>
    /// Gets or sets the unique identifier for this entity.
    /// </summary>
    /// <remarks>
    /// Uses a GUID to ensure uniqueness across distributed systems without coordination.
    /// </remarks>
    public Guid Id { get; protected set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseEntity"/> class with a new unique identifier.
    /// </summary>
    protected BaseEntity()
    {
        Id = Guid.NewGuid();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseEntity"/> class with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier for this entity.</param>
    /// <exception cref="ArgumentException">Thrown when id is empty.</exception>
    protected BaseEntity(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Entity Id cannot be empty.", nameof(id));
        }

        Id = id;
    }

    /// <summary>
    /// Determines whether the specified entity is equal to the current entity.
    /// </summary>
    /// <param name="other">The entity to compare with the current entity.</param>
    /// <returns>true if the specified entity has the same Id; otherwise, false.</returns>
    public bool Equals(BaseEntity? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (GetType() != other.GetType())
        {
            return false;
        }

        // Transient entities (with empty Id) are never equal
        if (Id == Guid.Empty || other.Id == Guid.Empty)
        {
            return false;
        }

        return Id == other.Id;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current entity.
    /// </summary>
    /// <param name="obj">The object to compare with the current entity.</param>
    /// <returns>true if the specified object is an entity with the same Id; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        return Equals(obj as BaseEntity);
    }

    /// <summary>
    /// Returns a hash code for this entity based on its Id.
    /// </summary>
    /// <returns>A hash code for the current entity.</returns>
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    /// <summary>
    /// Determines whether two entities are equal.
    /// </summary>
    public static bool operator ==(BaseEntity? left, BaseEntity? right)
    {
        if (left is null && right is null)
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two entities are not equal.
    /// </summary>
    public static bool operator !=(BaseEntity? left, BaseEntity? right)
    {
        return !(left == right);
    }
}
