namespace CodingAgentHelper.Api.Models;

/// <summary>
/// Request model for creating a new category
/// </summary>
public class CreateCategoryRequest
{
    /// <summary>
    /// Category name (required, max 100 characters, must be unique)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Category description (optional, max 500 characters)
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Request model for updating a category
/// </summary>
public class UpdateCategoryRequest
{
    /// <summary>
    /// New category description (optional)
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Response model for a category
/// </summary>
public class CategoryResponse
{
    /// <summary>
    /// Unique identifier for the category
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
    /// Creation timestamp (UTC)
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Paginated response for lists of categories
/// </summary>
public class CategoryListResponse
{
    /// <summary>
    /// List of categories
    /// </summary>
    public List<CategoryResponse> Items { get; set; } = new();

    /// <summary>
    /// Total count of categories
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
