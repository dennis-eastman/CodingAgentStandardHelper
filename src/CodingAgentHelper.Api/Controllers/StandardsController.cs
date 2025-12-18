namespace CodingAgentHelper.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using CodingAgentHelper.Core.Application.Services;
using CodingAgentHelper.Core.Domain.Entities;
using CodingAgentHelper.Api.Models;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// REST API controller for managing coding standards
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class StandardsController : ControllerBase
{
    private readonly IStandardService _standardService;
    private readonly ILogger<StandardsController> _logger;

    /// <summary>
    /// Initializes a new instance of the StandardsController
    /// </summary>
    public StandardsController(IStandardService standardService, ILogger<StandardsController> logger)
    {
        _standardService = standardService ?? throw new ArgumentNullException(nameof(standardService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all standards
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 10, max: 100)</param>
    /// <returns>Paginated list of standards</returns>
    /// <response code="200">Successfully retrieved standards</response>
    [HttpGet]
    [ProducesResponseType(typeof(StandardListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllStandards([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            pageSize = Math.Min(Math.Max(pageSize, 1), 100); // Clamp to 1-100
            pageNumber = Math.Max(pageNumber, 1);

            var standards = await _standardService.GetAllStandardsAsync();
            var standardList = standards.ToList();
            var totalCount = standardList.Count;

            var paginatedStandards = standardList
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(MapStandardToResponse)
                .ToList();

            var response = new StandardListResponse
            {
                Items = paginatedStandards,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            _logger.LogInformation("Retrieved {Count} standards (page {Page}/{Pages})", 
                paginatedStandards.Count, pageNumber, (totalCount + pageSize - 1) / pageSize);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving standards");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new ApiErrorResponse { StatusCode = 500, Message = "Error retrieving standards", Details = ex.Message });
        }
    }

    /// <summary>
    /// Get a specific standard by ID
    /// </summary>
    /// <param name="id">Standard ID</param>
    /// <returns>The requested standard</returns>
    /// <response code="200">Standard found</response>
    /// <response code="404">Standard not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(StandardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStandard([FromRoute] Guid id)
    {
        try
        {
            var standard = await _standardService.GetStandardAsync(id);
            if (standard == null)
            {
                _logger.LogWarning("Standard {StandardId} not found", id);
                return NotFound(new ApiErrorResponse 
                { 
                    StatusCode = 404, 
                    Message = $"Standard with ID {id} not found" 
                });
            }

            var response = MapStandardToResponse(standard);
            _logger.LogInformation("Retrieved standard {StandardId}", id);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving standard {StandardId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new ApiErrorResponse { StatusCode = 500, Message = "Error retrieving standard", Details = ex.Message });
        }
    }

    /// <summary>
    /// Create a new standard
    /// </summary>
    /// <param name="request">Standard creation request</param>
    /// <returns>Created standard</returns>
    /// <response code="201">Standard created successfully</response>
    /// <response code="400">Invalid request</response>
    [HttpPost]
    [ProducesResponseType(typeof(StandardResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateStandard([FromBody] CreateStandardRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiErrorResponse 
                { 
                    StatusCode = 400, 
                    Message = "Invalid request data",
                    Details = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))
                });
            }

            var standard = await _standardService.CreateStandardAsync(
                request.Title,
                request.Description,
                request.Category
            );

            if (request.Priority.HasValue)
            {
                await _standardService.UpdateStandardAsync(standard.Id, null, null, null, (StandardPriority)request.Priority.Value);
            }

            if (request.Tags != null && request.Tags.Count > 0)
            {
                foreach (var tag in request.Tags)
                {
                    await _standardService.AddTagToStandardAsync(standard.Id, tag);
                }
            }

            var response = MapStandardToResponse(standard);
            _logger.LogInformation("Created standard {StandardId}: {Title}", standard.Id, standard.Title);

            return CreatedAtAction(nameof(GetStandard), new { id = standard.Id }, response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid standard data");
            return BadRequest(new ApiErrorResponse { StatusCode = 400, Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating standard");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new ApiErrorResponse { StatusCode = 500, Message = "Error creating standard", Details = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing standard
    /// </summary>
    /// <param name="id">Standard ID</param>
    /// <param name="request">Standard update request</param>
    /// <returns>Updated standard</returns>
    /// <response code="200">Standard updated successfully</response>
    /// <response code="404">Standard not found</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(StandardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStandard([FromRoute] Guid id, [FromBody] UpdateStandardRequest request)
    {
        try
        {
            var standard = await _standardService.GetStandardAsync(id);
            if (standard == null)
            {
                _logger.LogWarning("Standard {StandardId} not found for update", id);
                return NotFound(new ApiErrorResponse 
                { 
                    StatusCode = 404, 
                    Message = $"Standard with ID {id} not found" 
                });
            }

            await _standardService.UpdateStandardAsync(
                id,
                null,
                null,
                request.Status.HasValue ? (StandardStatus)request.Status.Value : null,
                request.Priority.HasValue ? (StandardPriority)request.Priority.Value : null
            );

            standard = await _standardService.GetStandardAsync(id);
            var response = MapStandardToResponse(standard!);
            _logger.LogInformation("Updated standard {StandardId}", id);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating standard {StandardId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new ApiErrorResponse { StatusCode = 500, Message = "Error updating standard", Details = ex.Message });
        }
    }

    /// <summary>
    /// Delete a standard
    /// </summary>
    /// <param name="id">Standard ID</param>
    /// <returns>No content</returns>
    /// <response code="204">Standard deleted successfully</response>
    /// <response code="404">Standard not found</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteStandard([FromRoute] Guid id)
    {
        try
        {
            var standard = await _standardService.GetStandardAsync(id);
            if (standard == null)
            {
                _logger.LogWarning("Standard {StandardId} not found for deletion", id);
                return NotFound(new ApiErrorResponse 
                { 
                    StatusCode = 404, 
                    Message = $"Standard with ID {id} not found" 
                });
            }

            await _standardService.DeleteStandardAsync(id);
            _logger.LogInformation("Deleted standard {StandardId}", id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting standard {StandardId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new ApiErrorResponse { StatusCode = 500, Message = "Error deleting standard", Details = ex.Message });
        }
    }

    /// <summary>
    /// Search standards by query
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="category">Filter by category (optional)</param>
    /// <returns>Search results</returns>
    /// <response code="200">Search completed successfully</response>
    [HttpGet("search")]
    [ProducesResponseType(typeof(StandardListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchStandards([FromQuery] string query, [FromQuery] string? category = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest(new ApiErrorResponse 
                { 
                    StatusCode = 400, 
                    Message = "Search query cannot be empty" 
                });
            }

            var results = await _standardService.SearchStandardsAsync(query);

            if (!string.IsNullOrWhiteSpace(category))
            {
                results = results.Where(s => s.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
            }

            var standards = results.Select(MapStandardToResponse).ToList();
            var response = new StandardListResponse
            {
                Items = standards,
                TotalCount = standards.Count,
                PageNumber = 1,
                PageSize = standards.Count
            };

            _logger.LogInformation("Searched standards with query '{Query}', found {Count} results", query, standards.Count);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching standards with query '{Query}'", query);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new ApiErrorResponse { StatusCode = 500, Message = "Error searching standards", Details = ex.Message });
        }
    }

    /// <summary>
    /// Add a tag to a standard
    /// </summary>
    /// <param name="id">Standard ID</param>
    /// <param name="tag">Tag to add</param>
    /// <returns>Updated standard</returns>
    /// <response code="200">Tag added successfully</response>
    /// <response code="404">Standard not found</response>
    [HttpPost("{id}/tags")]
    [ProducesResponseType(typeof(StandardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddTag([FromRoute] Guid id, [FromBody] TagRequest tagRequest)
    {
        try
        {
            var standard = await _standardService.GetStandardAsync(id);
            if (standard == null)
            {
                _logger.LogWarning("Standard {StandardId} not found for adding tag", id);
                return NotFound(new ApiErrorResponse 
                { 
                    StatusCode = 404, 
                    Message = $"Standard with ID {id} not found" 
                });
            }

            if (string.IsNullOrWhiteSpace(tagRequest.Tag))
            {
                return BadRequest(new ApiErrorResponse 
                { 
                    StatusCode = 400, 
                    Message = "Tag cannot be empty" 
                });
            }

            await _standardService.AddTagToStandardAsync(id, tagRequest.Tag);
            standard = await _standardService.GetStandardAsync(id);

            var response = MapStandardToResponse(standard!);
            _logger.LogInformation("Added tag '{Tag}' to standard {StandardId}", tagRequest.Tag, id);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding tag to standard {StandardId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new ApiErrorResponse { StatusCode = 500, Message = "Error adding tag", Details = ex.Message });
        }
    }

    /// <summary>
    /// Remove a tag from a standard
    /// </summary>
    /// <param name="id">Standard ID</param>
    /// <param name="tag">Tag to remove</param>
    /// <returns>Updated standard</returns>
    /// <response code="200">Tag removed successfully</response>
    /// <response code="404">Standard not found</response>
    [HttpDelete("{id}/tags/{tag}")]
    [ProducesResponseType(typeof(StandardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveTag([FromRoute] Guid id, [FromRoute] string tag)
    {
        try
        {
            var standard = await _standardService.GetStandardAsync(id);
            if (standard == null)
            {
                _logger.LogWarning("Standard {StandardId} not found for removing tag", id);
                return NotFound(new ApiErrorResponse 
                { 
                    StatusCode = 404, 
                    Message = $"Standard with ID {id} not found" 
                });
            }

            if (string.IsNullOrWhiteSpace(tag))
            {
                return BadRequest(new ApiErrorResponse 
                { 
                    StatusCode = 400, 
                    Message = "Tag cannot be empty" 
                });
            }

            await _standardService.RemoveTagFromStandardAsync(id, tag);
            standard = await _standardService.GetStandardAsync(id);

            var response = MapStandardToResponse(standard!);
            _logger.LogInformation("Removed tag '{Tag}' from standard {StandardId}", tag, id);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing tag from standard {StandardId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new ApiErrorResponse { StatusCode = 500, Message = "Error removing tag", Details = ex.Message });
        }
    }

    /// <summary>
    /// Maps a Standard domain entity to a StandardResponse DTO
    /// </summary>
    private static StandardResponse MapStandardToResponse(Standard standard)
    {
        return new StandardResponse
        {
            Id = standard.Id,
            Title = standard.Title,
            Description = standard.Description,
            Category = standard.Category,
            Status = standard.Status.ToString(),
            Priority = standard.Priority.ToString(),
            Tags = standard.Tags,
            VectorId = standard.VectorId,
            CreatedAt = standard.CreatedAt,
            UpdatedAt = standard.UpdatedAt
        };
    }
}

/// <summary>
/// Request model for tag operations
/// </summary>
public class TagRequest
{
    /// <summary>
    /// Tag value
    /// </summary>
    public string Tag { get; set; } = string.Empty;
}
