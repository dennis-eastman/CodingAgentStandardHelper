namespace CodingAgentHelper.Web.Models;

/// <summary>
/// View model for displaying a single standard
/// </summary>
public class StandardViewModel
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Title of the standard
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Description
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
    /// Priority level
    /// </summary>
    public string Priority { get; set; } = string.Empty;

    /// <summary>
    /// Associated tags
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// View model for list of standards with pagination
/// </summary>
public class StandardListViewModel
{
    /// <summary>
    /// List of standards
    /// </summary>
    public List<StandardViewModel> Standards { get; set; } = new();

    /// <summary>
    /// Total count of standards
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page number
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Items per page
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Total pages
    /// </summary>
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;

    /// <summary>
    /// Search query (if any)
    /// </summary>
    public string? SearchQuery { get; set; }

    /// <summary>
    /// Category filter (if any)
    /// </summary>
    public string? CategoryFilter { get; set; }
}

/// <summary>
/// View model for creating a new standard
/// </summary>
public class StandardCreateViewModel
{
    /// <summary>
    /// Title (required)
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Description (required)
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Category (required)
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Priority (0=Low, 1=Medium, 2=High, 3=Critical)
    /// </summary>
    public int Priority { get; set; } = 1;

    /// <summary>
    /// Tags as comma-separated list
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// Available categories for dropdown
    /// </summary>
    public List<string> AvailableCategories { get; set; } = new();
}

/// <summary>
/// View model for editing a standard
/// </summary>
public class StandardEditViewModel
{
    /// <summary>
    /// Standard ID (not editable)
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Title (read-only in initial implementation)
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Description (read-only in initial implementation)
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Category (read-only in initial implementation)
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Status (0=Active, 1=Inactive, 2=Archived, 3=Deprecated)
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// Priority (0=Low, 1=Medium, 2=High, 3=Critical)
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// Tags as comma-separated list
    /// </summary>
    public string Tags { get; set; } = string.Empty;

    /// <summary>
    /// Available statuses for dropdown
    /// </summary>
    public List<(int Value, string Display)> AvailableStatuses => new()
    {
        (0, "Active"),
        (1, "Inactive"),
        (2, "Archived"),
        (3, "Deprecated")
    };

    /// <summary>
    /// Available priorities for dropdown
    /// </summary>
    public List<(int Value, string Display)> AvailablePriorities => new()
    {
        (0, "Low"),
        (1, "Medium"),
        (2, "High"),
        (3, "Critical")
    };
}

/// <summary>
/// View model for searching standards
/// </summary>
public class StandardSearchViewModel
{
    /// <summary>
    /// Search query
    /// </summary>
    public string? Query { get; set; }

    /// <summary>
    /// Category filter
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Status filter
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Priority filter
    /// </summary>
    public string? Priority { get; set; }

    /// <summary>
    /// Search results
    /// </summary>
    public List<StandardViewModel> Results { get; set; } = new();

    /// <summary>
    /// Result count
    /// </summary>
    public int ResultCount => Results.Count;
}
