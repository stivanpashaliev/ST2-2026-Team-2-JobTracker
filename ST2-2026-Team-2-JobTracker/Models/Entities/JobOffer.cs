using ST2_2026_Team_2_JobTracker.Models.Enums;

namespace ST2_2026_Team_2_JobTracker.Models.Entities;

public class JobOffer
{
    public int Id { get; set; }

    public int CompanyId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? Location { get; set; }

    public decimal? Salary { get; set; }

    public RemoteType RemoteType { get; set; } = RemoteType.OnSite;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Company Company { get; set; } = null!;

    public Application? Application { get; set; }
}