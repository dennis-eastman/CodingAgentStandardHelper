using CodingAgentHelper.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace CodingAgentHelper.Web.Pages.Admin.Standards;

/// <summary>
/// Razor Page for editing a standard's status, priority, and tags
/// </summary>
public class EditModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<EditModel> _logger;
    private const string ApiBaseUrl = "http://localhost:5000/api";

    /// <summary>
    /// The standard being edited
    /// </summary>
    public StandardViewModel Standard { get; set; } = new();

    /// <summary>
    /// Input model for editing
    /// </summary>
    [BindProperty]
    public StandardEditViewModel Input { get; set; } = new();

    /// <summary>
    /// Success message
    /// </summary>
    public string? SuccessMessage { get; set; }

    /// <summary>
    /// Error message
    /// </summary>
    public string? ErrorMessage { get; set; }

    public EditModel(IHttpClientFactory httpClientFactory, ILogger<EditModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// GET handler - Load standard for editing
    /// </summary>
    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{ApiBaseUrl}/standards/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true
                };

                Standard = JsonSerializer.Deserialize<StandardViewModel>(content, options) 
                    ?? new StandardViewModel();

                // Initialize edit model from loaded standard
                Input = new StandardEditViewModel
                {
                    Id = Standard.Id,
                    Title = Standard.Title,
                    Description = Standard.Description,
                    Category = Standard.Category,
                    Status = Standard.Status switch
                    {
                        "Active" => 0,
                        "Inactive" => 1,
                        "Archived" => 2,
                        "Deprecated" => 3,
                        _ => 0
                    },
                    Priority = Standard.Priority switch
                    {
                        "Low" => 0,
                        "Medium" => 1,
                        "High" => 2,
                        "Critical" => 3,
                        _ => 1
                    },
                    Tags = Standard.Tags?.Count > 0 ? string.Join(", ", Standard.Tags) : string.Empty
                };

                return Page();
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            else
            {
                _logger.LogError("Failed to fetch standard: {StatusCode}", response.StatusCode);
                return StatusCode((int)response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading standard");
            return StatusCode(500);
        }
    }

    /// <summary>
    /// POST handler - Save changes
    /// </summary>
    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            if (Input.Id == Guid.Empty)
            {
                return NotFound();
            }

            // Reload the standard to display while showing result
            await LoadStandardAsync(Input.Id);

            var client = _httpClientFactory.CreateClient();

            // Parse tags from comma-separated string
            var tags = string.IsNullOrWhiteSpace(Input.Tags)
                ? new List<string>()
                : Input.Tags.Split(',')
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .ToList();

            // Create update request
            var request = new
            {
                status = Input.Status,
                priority = Input.Priority,
                tags = tags
            };

            var content = new StringContent(
                JsonSerializer.Serialize(request),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await client.PutAsync($"{ApiBaseUrl}/standards/{Input.Id}", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Standard updated: {Id}", Input.Id);
                SuccessMessage = "Standard updated successfully!";
                // Reload the updated standard
                await LoadStandardAsync(Input.Id);
                return Page();
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                _logger.LogWarning("Bad request updating standard");
                ErrorMessage = "Invalid data provided. Please check your input.";
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            else
            {
                _logger.LogError("Failed to update standard: {StatusCode}", response.StatusCode);
                ErrorMessage = "Failed to update standard. Please try again.";
            }

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating standard");
            ErrorMessage = "An error occurred while updating the standard.";
            return Page();
        }
    }

    /// <summary>
    /// Helper method to load standard details
    /// </summary>
    private async Task LoadStandardAsync(Guid id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{ApiBaseUrl}/standards/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true
                };

                Standard = JsonSerializer.Deserialize<StandardViewModel>(content, options) 
                    ?? new StandardViewModel();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading standard for display");
        }
    }
}
