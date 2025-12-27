namespace Vendix.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when a requested entity is not found.
/// </summary>
public sealed class NotFoundException : Exception
{
    /// <summary>
    /// Gets the name of the entity type that was not found.
    /// </summary>
    public string EntityName { get; }

    /// <summary>
    /// Gets the identifier used to search for the entity.
    /// </summary>
    public object? Key { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class.
    /// </summary>
    public NotFoundException()
        : base("The requested entity was not found.")
    {
        EntityName = "Entity";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class with a message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public NotFoundException(string message)
        : base(message)
    {
        EntityName = "Entity";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class.
    /// </summary>
    /// <param name="entityName">The name of the entity type.</param>
    /// <param name="key">The identifier used to search for the entity.</param>
    public NotFoundException(string entityName, object key)
        : base($"Entity \"{entityName}\" ({key}) was not found.")
    {
        EntityName = entityName;
        Key = key;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class with an inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public NotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
        EntityName = "Entity";
    }

    /// <summary>
    /// Creates a NotFoundException for a specific entity type.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="key">The identifier used to search for the entity.</param>
    /// <returns>A new NotFoundException instance.</returns>
    public static NotFoundException ForEntity<T>(object key)
    {
        return new NotFoundException(typeof(T).Name, key);
    }
}
