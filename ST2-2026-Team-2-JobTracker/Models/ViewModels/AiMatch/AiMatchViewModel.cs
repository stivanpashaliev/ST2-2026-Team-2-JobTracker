using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using ST2_2026_Team_2_JobTracker.Common.Helpers;
using ST2_2026_Team_2_JobTracker.Models.AI;
using ST2_2026_Team_2_JobTracker.Models.Enums;

namespace ST2_2026_Team_2_JobTracker.Models.ViewModels.AiMatch;

public sealed class AiMatchViewModel
{
    private string _skillsAndExperience = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Please select a job offer.")]
    [Display(Name = "Job offer")]
    public int JobOfferId { get; set; }

    [Required(ErrorMessage = "Please enter your skills and experience.")]
    [StringLength(3000, MinimumLength = 15,
        ErrorMessage = "Skills and experience must be between {2} and {1} characters.")]
    [Display(Name = "Your skills and experience")]
    public string SkillsAndExperience
    {
        get => _skillsAndExperience;
        set => _skillsAndExperience = TextNormalization.Required(value);
    }

    [EnumDataType(typeof(MatchingMode), ErrorMessage = "Select a valid analysis mode.")]
    [Display(Name = "Matching method")]
    public MatchingMode Mode { get; set; } = MatchingMode.Gemini;

    public IReadOnlyList<SelectListItem> JobOffers { get; set; } = [];
    public MatchingResult? Result { get; set; }
}
