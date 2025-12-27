namespace Vendix.Domain.Common;

/// <summary>
/// Base class for value objects in the domain model.
/// </summary>
/// <remarks>
/// Value objects are immutable objects that are defined by their attributes rather than identity.
/// Two value objects are equal if all their properties are equal.
/// Examples: Money, Address, DateRange, Email.
/// </remarks>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>
    /// Gets the atomic values that define equality for this value object.
    /// </summary>
    /// <returns>An enumerable of the values that make up this value object.</returns>
    /// <remarks>
    /// Derived classes must implement this to return all properties that should be
    /// considered when comparing two instances for equality.
    /// </remarks>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    /// <summary>
    /// Determines whether the specified value object is equal to the current value object.
    /// </summary>
    /// <param name="other">The value object to compare with the current value object.</param>
    /// <returns>true if the specified value object has the same values; otherwise, false.</returns>
    public bool Equals(ValueObject? other)
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

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current value object.
    /// </summary>
    /// <param name="obj">The object to compare with the current value object.</param>
    /// <returns>true if the specified object is a value object with the same values; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        return Equals(obj as ValueObject);
    }

    /// <summary>
    /// Returns a hash code for this value object based on its equality components.
    /// </summary>
    /// <returns>A hash code for the current value object.</returns>
    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Aggregate(0, (current, component) =>
                HashCode.Combine(current, component?.GetHashCode() ?? 0));
    }

    /// <summary>
    /// Determines whether two value objects are equal.
    /// </summary>
    public static bool operator ==(ValueObject? left, ValueObject? right)
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
    /// Determines whether two value objects are not equal.
    /// </summary>
    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !(left == right);
    }
}
