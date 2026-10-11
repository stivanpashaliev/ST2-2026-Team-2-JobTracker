namespace ST2_2026_Team_2_JobTracker.Models.Entities;

public class Company
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Website { get; set; }

    public string? Location { get; set; }

    public ICollection<JobOffer> JobOffers { get; set; }
        = new List<JobOffer>();
}