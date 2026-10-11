using ST2_2026_Team_2_JobTracker.Models.Enums;

namespace ST2_2026_Team_2_JobTracker.Models.Entities;

public class Application
{
    public int Id { get; set; }

    public int JobOfferId { get; set; }

    public ApplicationStatus Status { get; set; }
        = ApplicationStatus.Saved;

    public DateTime? AppliedAt { get; set; }

    public string? Notes { get; set; }

    public JobOffer JobOffer { get; set; } = null!;
}