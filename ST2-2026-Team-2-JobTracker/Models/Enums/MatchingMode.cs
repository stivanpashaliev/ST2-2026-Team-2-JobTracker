using System.ComponentModel.DataAnnotations;

namespace ST2_2026_Team_2_JobTracker.Models.Enums;

public enum MatchingMode
{
    [Display(Name = "Gemini AI")]
    Gemini = 0,

    [Display(Name = "Rule-based (offline)")]
    RuleBased = 1
}
