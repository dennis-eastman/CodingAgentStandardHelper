namespace CodingAgentHelper.Api.Models;

/// <summary>
/// Standardized API error response
/// </summary>
public class ApiErrorResponse
{
    /// <summary>
    /// HTTP status code
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Error message describing what went wrong
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Detailed error information (optional)
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Error trace ID for logging (optional)
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>
    /// Timestamp when error occurred (UTC)
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Success response wrapper
/// </summary>
public class ApiResponse<T>
{
    /// <summary>
    /// Whether the operation was successful
    /// </summary>
    public bool Success { get; set; } = true;

    /// <summary>
    /// Response data
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Optional message
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Timestamp when response was created (UTC)
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Search query parameters
/// </summary>
public class SearchQuery
{
    /// <summary>
    /// Search term for full-text or semantic search
    /// </summary>
    public string? Query { get; set; }

    /// <summary>
    /// Filter by category name (optional)
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Filter by status (optional)
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Filter by priority (optional)
    /// </summary>
    public string? Priority { get; set; }

    /// <summary>
    /// Filter by tag (optional)
    /// </summary>
    public string? Tag { get; set; }

    /// <summary>
    /// Page number for pagination (default: 1)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Items per page for pagination (default: 10, max: 100)
    /// </summary>
    public int PageSize { get; set; } = 10;
}
