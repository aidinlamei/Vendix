namespace Vendix.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when there is a conflict with an existing resource.
/// </summary>
public sealed class ConflictException : Exception
{
    /// <summary>
    /// Gets the name of the conflicting entity type.
    /// </summary>
    public string? EntityName { get; }

    /// <summary>
    /// Gets the name of the conflicting property.
    /// </summary>
    public string? PropertyName { get; }

    /// <summary>
    /// Gets the conflicting value.
    /// </summary>
    public object? ConflictingValue { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConflictException"/> class.
    /// </summary>
    public ConflictException()
        : base("A conflict with an existing resource occurred.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConflictException"/> class with a message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public ConflictException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConflictException"/> class for a duplicate entry.
    /// </summary>
    /// <param name="entityName">The name of the entity type.</param>
    /// <param name="propertyName">The name of the conflicting property.</param>
    /// <param name="conflictingValue">The value that caused the conflict.</param>
    public ConflictException(string entityName, string propertyName, object conflictingValue)
        : base($"A {entityName} with {propertyName} '{conflictingValue}' already exists.")
    {
        EntityName = entityName;
        PropertyName = propertyName;
        ConflictingValue = conflictingValue;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConflictException"/> class with an inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public ConflictException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Creates a ConflictException for a duplicate entry.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="propertyName">The name of the conflicting property.</param>
    /// <param name="conflictingValue">The value that caused the conflict.</param>
    /// <returns>A new ConflictException instance.</returns>
    public static ConflictException ForDuplicate<T>(string propertyName, object conflictingValue)
    {
        return new ConflictException(typeof(T).Name, propertyName, conflictingValue);
    }
}
