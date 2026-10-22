using Microsoft.AspNetCore.Mvc;
using ST2_2026_Team_2_JobTracker.Services.Interfaces;

namespace ST2_2026_Team_2_JobTracker.Controllers;

public sealed class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = await _dashboardService.GetDashboardAsync(cancellationToken);
        return View(model);
    }
}
