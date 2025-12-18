namespace CodingAgentHelper.Core.Domain.Entities;

/// <summary>
/// Represents a category for organizing standards
/// </summary>
public class Category
{
    public Category(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        Id = Guid.NewGuid();
        Name = name;
        Description = description ?? string.Empty;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public int StandardCount { get; set; }
    public DateTime CreatedAt { get; private set; }

    public void UpdateDescription(string newDescription)
    {
        Description = newDescription ?? string.Empty;
    }
}
