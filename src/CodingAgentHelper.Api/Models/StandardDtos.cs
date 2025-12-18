namespace CodingAgentHelper.Api.Models;

/// <summary>
/// Request model for creating a new standard
/// </summary>
public class CreateStandardRequest
{
    /// <summary>
    /// Title of the standard (required, max 255 characters)
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the standard (required)
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Category for organizing the standard (required, max 100 characters)
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Priority level: Low=0, Medium=1, High=2, Critical=3 (optional, defaults to Medium)
    /// </summary>
    public int? Priority { get; set; }

    /// <summary>
    /// Initial tags for the standard (optional)
    /// </summary>
    public List<string>? Tags { get; set; }
}

/// <summary>
/// Request model for updating an existing standard
/// </summary>
public class UpdateStandardRequest
{
    /// <summary>
    /// New status: Active=0, Inactive=1, Archived=2, Deprecated=3 (optional)
    /// </summary>
    public int? Status { get; set; }

    /// <summary>
    /// New priority level (optional)
    /// </summary>
    public int? Priority { get; set; }
}

/// <summary>
/// Response model for a standard
/// </summary>
public class StandardResponse
{
    /// <summary>
    /// Unique identifier for the standard
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Title of the standard
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Category name
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Current status
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Current priority level
    /// </summary>
    public string Priority { get; set; } = string.Empty;

    /// <summary>
    /// Associated tags
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Vector ID in Chroma (if embedded)
    /// </summary>
    public string? VectorId { get; set; }

    /// <summary>
    /// Creation timestamp (UTC)
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last update timestamp (UTC)
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Paginated response for lists of standards
/// </summary>
public class StandardListResponse
{
    /// <summary>
    /// List of standards
    /// </summary>
    public List<StandardResponse> Items { get; set; } = new();

    /// <summary>
    /// Total count of standards
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page number
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Items per page
    /// </summary>
    public int PageSize { get; set; }
}
