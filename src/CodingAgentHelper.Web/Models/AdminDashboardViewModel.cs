namespace CodingAgentHelper.Web.Models;

/// <summary>
/// View model for admin dashboard
/// </summary>
public class AdminDashboardViewModel
{
    /// <summary>
    /// Total count of standards
    /// </summary>
    public int TotalStandards { get; set; }

    /// <summary>
    /// Total count of categories
    /// </summary>
    public int TotalCategories { get; set; }

    /// <summary>
    /// Count of active standards
    /// </summary>
    public int ActiveStandards { get; set; }

    /// <summary>
    /// Count of inactive standards
    /// </summary>
    public int InactiveStandards { get; set; }

    /// <summary>
    /// Count of archived standards
    /// </summary>
    public int ArchivedStandards { get; set; }

    /// <summary>
    /// Most recently added standards
    /// </summary>
    public List<StandardViewModel> RecentStandards { get; set; } = new();

    /// <summary>
    /// Top categories by standard count
    /// </summary>
    public List<CategoryViewModel> TopCategories { get; set; } = new();

    /// <summary>
    /// Standards by priority
    /// </summary>
    public Dictionary<string, int> StandardsByPriority { get; set; } = new();

    /// <summary>
    /// Standards by status
    /// </summary>
    public Dictionary<string, int> StandardsByStatus { get; set; } = new();
}
