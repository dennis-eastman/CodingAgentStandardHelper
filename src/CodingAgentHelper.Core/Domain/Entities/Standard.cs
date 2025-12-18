namespace CodingAgentHelper.Core.Domain.Entities;

/// <summary>
/// Represents a coding standard that the AI should follow
/// </summary>
public class Standard
{
    public Standard(string title, string description, string category)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty", nameof(title));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty", nameof(description));
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Category cannot be empty", nameof(category));

        Id = Guid.NewGuid();
        Title = title;
        Description = description;
        Category = category;
        Status = StandardStatus.Active;
        Priority = StandardPriority.Medium;
        Tags = new List<string>();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public string Category { get; private set; }
    public StandardStatus Status { get; private set; }
    public StandardPriority Priority { get; private set; }
    public List<string> Tags { get; private set; }
    public string? VectorId { get; set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public void UpdateStatus(StandardStatus newStatus)
    {
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePriority(StandardPriority newPriority)
    {
        Priority = newPriority;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddTag(string tag)
    {
        if (!string.IsNullOrWhiteSpace(tag) && !Tags.Contains(tag))
        {
            Tags.Add(tag);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void RemoveTag(string tag)
    {
        if (Tags.Remove(tag))
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
