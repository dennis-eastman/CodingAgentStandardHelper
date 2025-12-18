namespace CodingAgentHelper.Web.Models;

/// <summary>
/// View model for displaying a single category
/// </summary>
public class CategoryViewModel
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Category name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Category description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Number of standards in this category
    /// </summary>
    public int StandardCount { get; set; }

    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// View model for list of categories with pagination
/// </summary>
public class CategoryListViewModel
{
    /// <summary>
    /// List of categories
    /// </summary>
    public List<CategoryViewModel> Categories { get; set; } = new();

    /// <summary>
    /// Total count of categories
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page number
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Items per page
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Total pages
    /// </summary>
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
}

/// <summary>
/// View model for creating a new category
/// </summary>
public class CategoryCreateViewModel
{
    /// <summary>
    /// Category name (required, must be unique)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Category description (optional)
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// View model for editing a category
/// </summary>
public class CategoryEditViewModel
{
    /// <summary>
    /// Category ID (not editable)
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Category name (read-only)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Category description (editable)
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Number of standards in this category
    /// </summary>
    public int StandardCount { get; set; }
}
