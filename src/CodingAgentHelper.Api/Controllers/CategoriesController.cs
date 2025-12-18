namespace CodingAgentHelper.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using CodingAgentHelper.Core.Application.Services;
using CodingAgentHelper.Core.Domain.Entities;
using CodingAgentHelper.Api.Models;

/// <summary>
/// REST API controller for managing categories
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CategoriesController> _logger;

    /// <summary>
    /// Initializes a new instance of the CategoriesController
    /// </summary>
    public CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger)
    {
        _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all categories
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 10, max: 100)</param>
    /// <returns>Paginated list of categories</returns>
    /// <response code="200">Successfully retrieved categories</response>
    [HttpGet]
    [ProducesResponseType(typeof(CategoryListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllCategories([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            pageSize = Math.Min(Math.Max(pageSize, 1), 100); // Clamp to 1-100
            pageNumber = Math.Max(pageNumber, 1);

            var categories = await _categoryService.GetCategoriesWithStandardCountAsync();
            var categoryList = categories.ToList();
            var totalCount = categoryList.Count;

            var paginatedCategories = categoryList
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(MapCategoryToResponse)
                .ToList();

            var response = new CategoryListResponse
            {
                Items = paginatedCategories,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            _logger.LogInformation("Retrieved {Count} categories (page {Page}/{Pages})", 
                paginatedCategories.Count, pageNumber, (totalCount + pageSize - 1) / pageSize);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving categories");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new ApiErrorResponse { StatusCode = 500, Message = "Error retrieving categories", Details = ex.Message });
        }
    }

    /// <summary>
    /// Get a specific category by ID
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <returns>The requested category</returns>
    /// <response code="200">Category found</response>
    /// <response code="404">Category not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategory([FromRoute] Guid id)
    {
        try
        {
            var category = await _categoryService.GetCategoryAsync(id);
            if (category == null)
            {
                _logger.LogWarning("Category {CategoryId} not found", id);
                return NotFound(new ApiErrorResponse 
                { 
                    StatusCode = 404, 
                    Message = $"Category with ID {id} not found" 
                });
            }

            var response = MapCategoryToResponse(category);
            _logger.LogInformation("Retrieved category {CategoryId}", id);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category {CategoryId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new ApiErrorResponse { StatusCode = 500, Message = "Error retrieving category", Details = ex.Message });
        }
    }

    /// <summary>
    /// Create a new category
    /// </summary>
    /// <param name="request">Category creation request</param>
    /// <returns>Created category</returns>
    /// <response code="201">Category created successfully</response>
    /// <response code="400">Invalid request or category name already exists</response>
    [HttpPost]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
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

            var category = await _categoryService.CreateCategoryAsync(
                request.Name,
                request.Description ?? string.Empty
            );

            var response = MapCategoryToResponse(category);
            _logger.LogInformation("Created category {CategoryId}: {Name}", category.Id, category.Name);

            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid category data");
            return BadRequest(new ApiErrorResponse { StatusCode = 400, Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Category creation failed");
            return BadRequest(new ApiErrorResponse { StatusCode = 400, Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new ApiErrorResponse { StatusCode = 500, Message = "Error creating category", Details = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing category
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <param name="request">Category update request</param>
    /// <returns>Updated category</returns>
    /// <response code="200">Category updated successfully</response>
    /// <response code="404">Category not found</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCategory([FromRoute] Guid id, [FromBody] UpdateCategoryRequest request)
    {
        try
        {
            var category = await _categoryService.GetCategoryAsync(id);
            if (category == null)
            {
                _logger.LogWarning("Category {CategoryId} not found for update", id);
                return NotFound(new ApiErrorResponse 
                { 
                    StatusCode = 404, 
                    Message = $"Category with ID {id} not found" 
                });
            }

            await _categoryService.UpdateCategoryAsync(id, request.Description ?? category.Description);

            category = await _categoryService.GetCategoryAsync(id);
            var response = MapCategoryToResponse(category!);
            _logger.LogInformation("Updated category {CategoryId}", id);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category {CategoryId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new ApiErrorResponse { StatusCode = 500, Message = "Error updating category", Details = ex.Message });
        }
    }

    /// <summary>
    /// Delete a category
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <returns>No content</returns>
    /// <response code="204">Category deleted successfully</response>
    /// <response code="404">Category not found</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCategory([FromRoute] Guid id)
    {
        try
        {
            var category = await _categoryService.GetCategoryAsync(id);
            if (category == null)
            {
                _logger.LogWarning("Category {CategoryId} not found for deletion", id);
                return NotFound(new ApiErrorResponse 
                { 
                    StatusCode = 404, 
                    Message = $"Category with ID {id} not found" 
                });
            }

            await _categoryService.DeleteCategoryAsync(id);
            _logger.LogInformation("Deleted category {CategoryId}", id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category {CategoryId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new ApiErrorResponse { StatusCode = 500, Message = "Error deleting category", Details = ex.Message });
        }
    }

    /// <summary>
    /// Maps a Category domain entity to a CategoryResponse DTO
    /// </summary>
    private static CategoryResponse MapCategoryToResponse(Category category)
    {
        return new CategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            StandardCount = category.StandardCount,
            CreatedAt = category.CreatedAt
        };
    }
}
