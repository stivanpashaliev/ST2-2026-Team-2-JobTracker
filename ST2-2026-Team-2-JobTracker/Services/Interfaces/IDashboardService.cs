using ST2_2026_Team_2_JobTracker.Models.ViewModels.Dashboard;

namespace ST2_2026_Team_2_JobTracker.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardViewModel> GetDashboardAsync(CancellationToken cancellationToken = default);
}
