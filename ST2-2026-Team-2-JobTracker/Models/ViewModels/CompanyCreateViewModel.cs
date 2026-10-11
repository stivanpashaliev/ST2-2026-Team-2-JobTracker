using System.ComponentModel.DataAnnotations;
using ST2_2026_Team_2_JobTracker.Common.Constants;

namespace ST2_2026_Team_2_JobTracker.Models.ViewModels;

public class CompanyCreateViewModel
{
    [Required(ErrorMessage = "Company name is required.")]
    [StringLength(
        ValidationConstants.Company.NameMaxLength,
        MinimumLength = ValidationConstants.Company.NameMinLength,
        ErrorMessage = "Company name must be between {2} and {1} characters.")]
    public string Name { get; set; } = string.Empty;

    [Url(ErrorMessage = "Please enter a valid website URL.")]
    [StringLength(
        ValidationConstants.Company.WebsiteMaxLength,
        ErrorMessage = "Website URL cannot exceed {1} characters.")]
    public string? Website { get; set; }

    [StringLength(
        ValidationConstants.Company.LocationMaxLength,
        ErrorMessage = "Location cannot exceed {1} characters.")]
    public string? Location { get; set; }
}

