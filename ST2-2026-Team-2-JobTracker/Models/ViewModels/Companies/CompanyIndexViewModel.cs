
using ST2_2026_Team_2_JobTracker.Models.Entities;

namespace ST2_2026_Team_2_JobTracker.Models.ViewModels;

public class CompanyIndexViewModel
{
    public string? Name { get; set; }

    public string? Location { get; set; }

    public List<Company> Companies { get; set; } = new();
}
