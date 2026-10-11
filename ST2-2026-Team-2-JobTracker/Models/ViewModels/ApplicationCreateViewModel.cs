using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using ST2_2026_Team_2_JobTracker.Common.Constants;
using ST2_2026_Team_2_JobTracker.Models.Enums;

namespace ST2_2026_Team_2_JobTracker.Models.ViewModels;


public class ApplicationCreateViewModel
{
    [Range(1, int.MaxValue,
        ErrorMessage = "Please select a job offer.")]
    public int JobOfferId { get; set; }

    [EnumDataType(
        typeof(ApplicationStatus),
        ErrorMessage = "Please select a valid application status.")]
    public ApplicationStatus Status { get; set; }
        = ApplicationStatus.Saved;

    [DataType(DataType.Date,
        ErrorMessage = "Please enter a valid application date.")]
    public DateTime? AppliedAt { get; set; }

    [StringLength(
        ValidationConstants.Application.NotesMaxLength,
        ErrorMessage = "Notes cannot exceed {1} characters.")]
    public string? Notes { get; set; }

    public IEnumerable<SelectListItem> JobOffers { get; set; }
        = new List<SelectListItem>();
}

