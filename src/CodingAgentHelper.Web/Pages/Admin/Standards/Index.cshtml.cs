using CodingAgentHelper.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CodingAgentHelper.Web.Pages.Admin.Standards;

/// <summary>
/// Razor Page for listing all standards with pagination and search
/// </summary>
public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<IndexModel> _logger;
    private const string ApiBaseUrl = "http://localhost:5000/api";

    /// <summary>
    /// List of standards to display
    /// </summary>
    public StandardListViewModel Standards { get; set; } = new();

    /// <summary>
    /// Search query from URL parameter
    /// </summary>
    public string? SearchQuery { get; set; }

    /// <summary>
    /// Current page number
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Items per page
    /// </summary>
    public int PageSize { get; set; } = 10;

    public IndexModel(IHttpClientFactory httpClientFactory, ILogger<IndexModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// GET handler - Load standards list
    /// </summary>
    public async Task OnGetAsync(string? searchQuery, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            SearchQuery = searchQuery;
            PageNumber = pageNumber;
            PageSize = pageSize;

            var client = _httpClientFactory.CreateClient();
            
            // Build query string
            var queryParams = new List<string>
            {
                $"pageNumber={pageNumber}",
                $"pageSize={pageSize}"
            };

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                queryParams.Add($"query={Uri.EscapeDataString(searchQuery)}");
            }

            var url = searchQuery != null 
                ? $"{ApiBaseUrl}/standards/search?{string.Join("&", queryParams)}"
                : $"{ApiBaseUrl}/standards?{string.Join("&", queryParams)}";

            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var options = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true
                };

                Standards = System.Text.Json.JsonSerializer.Deserialize<StandardListViewModel>(content, options) 
                    ?? new StandardListViewModel();
            }
            else
            {
                _logger.LogWarning("Failed to fetch standards: {StatusCode}", response.StatusCode);
                Standards = new StandardListViewModel();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading standards");
            Standards = new StandardListViewModel();
        }
    }
}
