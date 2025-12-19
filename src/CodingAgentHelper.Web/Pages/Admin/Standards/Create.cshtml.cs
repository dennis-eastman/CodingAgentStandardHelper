using CodingAgentHelper.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace CodingAgentHelper.Web.Pages.Admin.Standards;

/// <summary>
/// Razor Page for creating a new standard
/// </summary>
public class CreateModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<CreateModel> _logger;
    private const string ApiBaseUrl = "http://localhost:5000/api";

    /// <summary>
    /// Input model for creating standard
    /// </summary>
    [BindProperty]
    public StandardCreateViewModel Input { get; set; } = new();

    /// <summary>
    /// Success message after creation
    /// </summary>
    public string? SuccessMessage { get; set; }

    /// <summary>
    /// Error message if creation fails
    /// </summary>
    public string? ErrorMessage { get; set; }

    public CreateModel(IHttpClientFactory httpClientFactory, ILogger<CreateModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// GET handler - Display create form
    /// </summary>
    public void OnGet()
    {
        // Initialize empty form
        Input = new StandardCreateViewModel();
    }

    /// <summary>
    /// POST handler - Create new standard
    /// </summary>
    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Please fill in all required fields correctly.";
                return Page();
            }

            var client = _httpClientFactory.CreateClient();

            // Parse tags from comma-separated string
            var tags = string.IsNullOrWhiteSpace(Input.Tags)
                ? new List<string>()
                : Input.Tags.Split(',')
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .ToList();

            // Create request payload
            var request = new
            {
                title = Input.Title,
                description = Input.Description,
                category = Input.Category,
                priority = Input.Priority,
                tags = tags
            };

            var content = new StringContent(
                JsonSerializer.Serialize(request),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync($"{ApiBaseUrl}/standards", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Standard created: {Title}", Input.Title);
                return RedirectToPage("Index", new { message = "Standard created successfully!" });
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Bad request creating standard: {Error}", errorContent);
                ErrorMessage = "Invalid data provided. Please check your input and try again.";
            }
            else
            {
                _logger.LogError("Failed to create standard: {StatusCode}", response.StatusCode);
                ErrorMessage = "Failed to create standard. Please try again.";
            }

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating standard");
            ErrorMessage = "An error occurred while creating the standard.";
            return Page();
        }
    }
}
