using CodingAgentHelper.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace CodingAgentHelper.Web.Pages.Admin.Standards;

/// <summary>
/// Razor Page for viewing standard details
/// </summary>
public class DetailsModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<DetailsModel> _logger;
    private const string ApiBaseUrl = "http://localhost:5000/api";

    /// <summary>
    /// The standard being viewed
    /// </summary>
    public StandardViewModel Standard { get; set; } = new();

    public DetailsModel(IHttpClientFactory httpClientFactory, ILogger<DetailsModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// GET handler - Load standard details
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
                _logger.LogWarning("Standard not found: {Id}", id);
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
            _logger.LogError(ex, "Error loading standard details");
            return StatusCode(500);
        }
    }
}
