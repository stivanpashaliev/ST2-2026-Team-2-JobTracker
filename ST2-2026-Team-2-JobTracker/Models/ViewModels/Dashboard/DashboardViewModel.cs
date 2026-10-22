using ST2_2026_Team_2_JobTracker.Models.Enums;

namespace ST2_2026_Team_2_JobTracker.Models.ViewModels.Dashboard;

// Read-only projections for the Dashboard page. No EF entities are passed to Razor.
public sealed class DashboardViewModel
{
    public int TotalCompanies { get; init; }
    public int TotalJobOffers { get; init; }
    public int TotalApplications { get; init; }
    public int AvailableJobOffers { get; init; }

    public IReadOnlyList<DashboardStatusViewModel> ApplicationStatuses { get; init; }
        = Array.Empty<DashboardStatusViewModel>();

    public IReadOnlyList<DashboardRecentApplicationViewModel> RecentApplications { get; init; }
        = Array.Empty<DashboardRecentApplicationViewModel>();
}

public sealed class DashboardStatusViewModel
{
    public ApplicationStatus Status { get; init; }
    public int Count { get; init; }
}

public sealed class DashboardRecentApplicationViewModel
{
    public int Id { get; init; }
    public string JobTitle { get; init; } = string.Empty;
    public string CompanyName { get; init; } = string.Empty;
    public ApplicationStatus Status { get; init; }
    public DateTime? AppliedAt { get; init; }
}
