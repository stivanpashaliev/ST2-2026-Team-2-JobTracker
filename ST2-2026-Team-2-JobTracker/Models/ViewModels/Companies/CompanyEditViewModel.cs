using System.ComponentModel.DataAnnotations;

namespace ST2_2026_Team_2_JobTracker.Models.ViewModels.Companies;

public class CompanyEditViewModel : CompanyCreateViewModel
{
    [Range(1, int.MaxValue)]
    public int Id { get; set; }
}
