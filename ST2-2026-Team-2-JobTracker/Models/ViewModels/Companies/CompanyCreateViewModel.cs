using System.ComponentModel.DataAnnotations;
using ST2_2026_Team_2_JobTracker.Common.Constants;
using ST2_2026_Team_2_JobTracker.Common.Helpers;

namespace ST2_2026_Team_2_JobTracker.Models.ViewModels.Companies;

public class CompanyCreateViewModel
{
    private string _name = string.Empty;
    private string? _website;
    private string? _location;

    [Required(ErrorMessage = "Company name is required.")]
    [StringLength(
        ValidationConstants.Company.NameMaxLength,
        MinimumLength = ValidationConstants.Company.NameMinLength,
        ErrorMessage = "Company name must be between {2} and {1} characters.")]
    public string Name
    {
        get => _name;
        set => _name = TextNormalization.Required(value);
    }

    [Url(ErrorMessage = "Please enter a valid website URL.")]
    [StringLength(
        ValidationConstants.Company.WebsiteMaxLength,
        ErrorMessage = "Website URL cannot exceed {1} characters.")]
    public string? Website
    {
        get => _website;
        set => _website = TextNormalization.Optional(value);
    }

    [StringLength(
        ValidationConstants.Company.LocationMaxLength,
        ErrorMessage = "Location cannot exceed {1} characters.")]
    public string? Location
    {
        get => _location;
        set => _location = TextNormalization.Optional(value);
    }
}

