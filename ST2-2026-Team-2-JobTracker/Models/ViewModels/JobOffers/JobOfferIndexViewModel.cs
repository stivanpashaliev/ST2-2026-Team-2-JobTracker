
using Microsoft.AspNetCore.Mvc.Rendering;
using ST2_2026_Team_2_JobTracker.Models.Entities;
using ST2_2026_Team_2_JobTracker.Models.Enums;

namespace ST2_2026_Team_2_JobTracker.Models.ViewModels.JobOffers;

public class JobOfferIndexViewModel
{
    public string? Title { get; set; }

    public int? CompanyId { get; set; }

    public string? Location { get; set; }

    public RemoteType? RemoteType { get; set; }

    public List<JobOffer> JobOffers { get; set; } = new();

    public IEnumerable<SelectListItem> Companies { get; set; }
        = new List<SelectListItem>();
}
