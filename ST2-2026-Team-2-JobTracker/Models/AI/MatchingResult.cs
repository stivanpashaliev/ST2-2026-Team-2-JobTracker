namespace ST2_2026_Team_2_JobTracker.Models.AI;

// Immutable result passed to MVC; generated text is HTML-encoded by Razor when rendered.
public sealed class MatchingResult
{
    public required int Score { get; init; }
    public required string Summary { get; init; }
    public required string Source { get; init; }
    public IReadOnlyList<string> MatchingSkills { get; init; } = [];
    public IReadOnlyList<string> MissingSkills { get; init; } = [];
    public IReadOnlyList<string> Recommendations { get; init; } = [];
}
