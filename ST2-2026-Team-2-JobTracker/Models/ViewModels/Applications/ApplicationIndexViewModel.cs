
using Microsoft.AspNetCore.Mvc.Rendering;
using ST2_2026_Team_2_JobTracker.Models.Entities;
using ST2_2026_Team_2_JobTracker.Models.Enums;

namespace ST2_2026_Team_2_JobTracker.Models.ViewModels.Applications;

public class ApplicationIndexViewModel
{
    public string? JobTitle { get; set; }

    public int? CompanyId { get; set; }

    public ApplicationStatus? Status { get; set; }

    public List<Application> Applications { get; set; } = new();

    public IEnumerable<SelectListItem> Companies { get; set; }
        = new List<SelectListItem>();

    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
}
