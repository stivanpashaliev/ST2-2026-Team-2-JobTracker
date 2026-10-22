using System.Text.RegularExpressions;
using ST2_2026_Team_2_JobTracker.Models.AI;
using ST2_2026_Team_2_JobTracker.Models.Enums;

namespace ST2_2026_Team_2_JobTracker.DesignPatterns.Strategy;

/// <summary>
/// STRATEGY implementation: deterministic, offline skill-keyword comparison.
/// Does NOT call an LLM and must be labeled separately from the Gemini analysis.
/// </summary>
public sealed class RuleBasedMatchingStrategy : IMatchingStrategy
{
    private static readonly string[] Technologies =
    [
        "C#", "ASP.NET Core", "Entity Framework", "SQL Server", "PostgreSQL",
        "Python", "Java", "Spring Boot", "JavaScript", "TypeScript",
        "React", "Angular", "Node.js", "Docker", "Kubernetes",
        "Azure", "AWS", "Git", "REST API", "HTML", "CSS"
    ];

    public MatchingMode Mode => MatchingMode.RuleBased;

    // Word boundaries avoid false matches such as "Java" inside "JavaScript".
    private static bool ContainsSkill(string text, string skill) =>
        Regex.IsMatch(text, $@"(?<![A-Za-z0-9]){Regex.Escape(skill)}(?![A-Za-z0-9])",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public Task<MatchingResult> MatchAsync(
        MatchingInput input, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var required = Technologies.Where(skill =>
            ContainsSkill(input.JobDescription, skill)).ToArray();
        var matching = required.Where(skill =>
            ContainsSkill(input.CandidateProfile, skill)).ToArray();
        var missing = required.Except(matching).ToArray();
        var score = required.Length == 0
            ? 0 : (int)Math.Round(100.0 * matching.Length / required.Length);

        var summary = required.Length == 0
            ? "No supported technical keywords were found in this job description. The offline score is not informative."
            : $"Matched {matching.Length} of {required.Length} recognized technologies mentioned in the job description.";

        return Task.FromResult(new MatchingResult
        {
            Score = score,
            Summary = summary,
            Source = "Rule-based (offline)",
            MatchingSkills = matching,
            MissingSkills = missing,
            Recommendations = missing.Length == 0
                ? ["Review the full job requirements and prepare concrete examples of your experience."]
                : missing.Take(5).Select(skill => $"Consider learning or practicing {skill}.").ToArray()
        });
    }
}
