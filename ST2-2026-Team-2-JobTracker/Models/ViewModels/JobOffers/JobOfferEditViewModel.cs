using System.ComponentModel.DataAnnotations;

namespace ST2_2026_Team_2_JobTracker.Models.ViewModels.JobOffers;

public class JobOfferEditViewModel : JobOfferCreateViewModel
{
    [Range(1, int.MaxValue)]
    public int Id { get; set; }
}
