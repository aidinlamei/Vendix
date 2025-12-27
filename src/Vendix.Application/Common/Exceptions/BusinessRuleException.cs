namespace Vendix.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when a business rule is violated.
/// </summary>
public sealed class BusinessRuleException : Exception
{
    /// <summary>
    /// Gets the rule that was violated.
    /// </summary>
    public string? RuleName { get; }

    /// <summary>
    /// Gets additional details about the violation.
    /// </summary>
    public object? Details { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessRuleException"/> class.
    /// </summary>
    public BusinessRuleException()
        : base("A business rule was violated.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessRuleException"/> class with a message.
    /// </summary>
    /// <param name="message">The error message describing the violated rule.</param>
    public BusinessRuleException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessRuleException"/> class with a rule name.
    /// </summary>
    /// <param name="ruleName">The name of the violated rule.</param>
    /// <param name="message">The error message describing the violated rule.</param>
    public BusinessRuleException(string ruleName, string message)
        : base(message)
    {
        RuleName = ruleName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessRuleException"/> class with details.
    /// </summary>
    /// <param name="ruleName">The name of the violated rule.</param>
    /// <param name="message">The error message describing the violated rule.</param>
    /// <param name="details">Additional details about the violation.</param>
    public BusinessRuleException(string ruleName, string message, object details)
        : base(message)
    {
        RuleName = ruleName;
        Details = details;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessRuleException"/> class with an inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public BusinessRuleException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
