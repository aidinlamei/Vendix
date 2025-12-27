namespace Vendix.Domain.Common;

/// <summary>
/// Base class for aggregate roots in the domain model.
/// </summary>
/// <remarks>
/// An aggregate root is an entity that serves as the entry point to an aggregate.
/// It maintains the consistency of the aggregate and is the only member that external
/// objects can hold references to. Domain events can be raised to signal important
/// business occurrences within the aggregate.
/// </remarks>
public abstract class AggregateRoot : BaseEntity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Gets the collection of domain events raised by this aggregate.
    /// </summary>
    /// <remarks>
    /// Domain events are dispatched after the aggregate is persisted.
    /// This ensures consistency between the aggregate state and the events.
    /// </remarks>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRoot"/> class.
    /// </summary>
    protected AggregateRoot() : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRoot"/> class with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier for this aggregate root.</param>
    protected AggregateRoot(Guid id) : base(id)
    {
    }

    /// <summary>
    /// Adds a domain event to be dispatched when the aggregate is persisted.
    /// </summary>
    /// <param name="domainEvent">The domain event to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when domainEvent is null.</exception>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Removes a domain event from the collection.
    /// </summary>
    /// <param name="domainEvent">The domain event to remove.</param>
    protected void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    /// <summary>
    /// Clears all domain events from the collection.
    /// </summary>
    /// <remarks>
    /// Typically called by the infrastructure after events have been dispatched.
    /// </remarks>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

/// <summary>
/// Marker interface for domain events.
/// </summary>
/// <remarks>
/// Domain events represent something that has happened in the domain that domain experts care about.
/// They are named in past tense (e.g., OrderPlaced, ProductCreated) and are immutable.
/// </remarks>
public interface IDomainEvent
{
    /// <summary>
    /// Gets the date and time when this event occurred.
    /// </summary>
    DateTime OccurredOn { get; }
}

/// <summary>
/// Base class for domain events providing common functionality.
/// </summary>
public abstract record DomainEventBase : IDomainEvent
{
    /// <summary>
    /// Gets the date and time when this event occurred.
    /// </summary>
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
