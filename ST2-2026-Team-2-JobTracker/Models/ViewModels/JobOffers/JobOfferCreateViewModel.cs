using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using ST2_2026_Team_2_JobTracker.Common.Constants;
using ST2_2026_Team_2_JobTracker.Common.Helpers;
using ST2_2026_Team_2_JobTracker.Models.Enums;

namespace ST2_2026_Team_2_JobTracker.Models.ViewModels.JobOffers;


public class JobOfferCreateViewModel
{
    private string _title = string.Empty;
    private string _description = string.Empty;
    private string? _location;

    [Range(1, int.MaxValue,
        ErrorMessage = "Please select a company.")]
    public int CompanyId { get; set; }

    [Required(ErrorMessage = "Job title is required.")]
    [StringLength(
        ValidationConstants.JobOffer.TitleMaxLength,
        MinimumLength = ValidationConstants.JobOffer.TitleMinLength,
        ErrorMessage = "Job title must be between {2} and {1} characters.")]
    public string Title
    {
        get => _title;
        set => _title = TextNormalization.Required(value);
    }

    [Required(ErrorMessage = "Job description is required.")]
    [StringLength(
        ValidationConstants.JobOffer.DescriptionMaxLength,
        MinimumLength = ValidationConstants.JobOffer.DescriptionMinLength,
        ErrorMessage = "Job description must be between {2} and {1} characters.")]
    public string Description
    {
        get => _description;
        set => _description = TextNormalization.Required(value);
    }

    [StringLength(
        ValidationConstants.JobOffer.LocationMaxLength,
        ErrorMessage = "Location cannot exceed {1} characters.")]
    public string? Location
    {
        get => _location;
        set => _location = TextNormalization.Optional(value);
    }

    [Range(
        ValidationConstants.JobOffer.SalaryMinValue,
        ValidationConstants.JobOffer.SalaryMaxValue,
        ErrorMessage = "Salary must be between {1} and {2}.")]
    public decimal? Salary { get; set; }

    [Required(ErrorMessage = "Please select a remote work type.")]
    [EnumDataType(
        typeof(RemoteType),
        ErrorMessage = "Please select a valid remote work type.")]
    public RemoteType RemoteType { get; set; }

    public IEnumerable<SelectListItem> Companies { get; set; }
        = new List<SelectListItem>();
}
