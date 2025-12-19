using CodingAgentHelper.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace CodingAgentHelper.Web.Pages.Admin.Standards;

/// <summary>
/// Razor Page for deleting a standard
/// </summary>
public class DeleteModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<DeleteModel> _logger;
    private const string ApiBaseUrl = "http://localhost:5000/api";

    /// <summary>
    /// The standard to be deleted
    /// </summary>
    public StandardViewModel Standard { get; set; } = new();

    /// <summary>
    /// Error message if deletion fails
    /// </summary>
    public string? ErrorMessage { get; set; }

    public DeleteModel(IHttpClientFactory httpClientFactory, ILogger<DeleteModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// GET handler - Show delete confirmation
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
            _logger.LogError(ex, "Error loading standard for deletion");
            return StatusCode(500);
        }
    }

    /// <summary>
    /// POST handler - Perform deletion
    /// </summary>
    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            var client = _httpClientFactory.CreateClient();
            var response = await client.DeleteAsync($"{ApiBaseUrl}/standards/{id}");

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Standard deleted: {Id}", id);
                return RedirectToPage("Index", new { message = "Standard deleted successfully!" });
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            else
            {
                _logger.LogError("Failed to delete standard: {StatusCode}", response.StatusCode);
                ErrorMessage = "Failed to delete the standard. Please try again.";

                // Reload the standard to re-display the form
                var getResponse = await client.GetAsync($"{ApiBaseUrl}/standards/{id}");
                if (getResponse.IsSuccessStatusCode)
                {
                    var content = await getResponse.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        PropertyNameCaseInsensitive = true
                    };

                    Standard = JsonSerializer.Deserialize<StandardViewModel>(content, options) 
                        ?? new StandardViewModel();
                }

                return Page();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting standard");
            ErrorMessage = "An error occurred while deleting the standard.";
            return Page();
        }
    }
}
