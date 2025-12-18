namespace CodingAgentHelper.Core.Domain.Exceptions;

/// <summary>
/// Exception thrown when a domain rule is violated
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when an entity is not found
/// </summary>
public class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string entityName, object id)
        : base($"{entityName} with id '{id}' was not found")
    {
        EntityName = entityName;
        EntityId = id;
    }

    public string EntityName { get; }
    public object EntityId { get; }
}

/// <summary>
/// Exception thrown when an invalid operation is attempted
/// </summary>
public class InvalidOperationException : DomainException
{
    public InvalidOperationException(string message) : base(message)
    {
    }
}
