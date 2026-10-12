using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using ST2_2026_Team_2_JobTracker.Common.Constants;
using ST2_2026_Team_2_JobTracker.Models.Enums;

namespace ST2_2026_Team_2_JobTracker.Models.ViewModels.JobOffers;


public class JobOfferCreateViewModel
{
    [Range(1, int.MaxValue,
        ErrorMessage = "Please select a company.")]
    public int CompanyId { get; set; }

    [Required(ErrorMessage = "Job title is required.")]
    [StringLength(
        ValidationConstants.JobOffer.TitleMaxLength,
        MinimumLength = ValidationConstants.JobOffer.TitleMinLength,
        ErrorMessage = "Job title must be between {2} and {1} characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Job description is required.")]
    [StringLength(
        ValidationConstants.JobOffer.DescriptionMaxLength,
        MinimumLength = ValidationConstants.JobOffer.DescriptionMinLength,
        ErrorMessage = "Job description must be between {2} and {1} characters.")]
    public string Description { get; set; } = string.Empty;

    [StringLength(
        ValidationConstants.JobOffer.LocationMaxLength,
        ErrorMessage = "Location cannot exceed {1} characters.")]
    public string? Location { get; set; }

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
