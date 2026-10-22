using ST2_2026_Team_2_JobTracker.Models.AI;
using ST2_2026_Team_2_JobTracker.Models.Enums;

namespace ST2_2026_Team_2_JobTracker.DesignPatterns.Strategy;

/// <summary>
/// STRATEGY PATTERN: interchangeable matching algorithms share one contract.
/// Adding a new provider does not change AiMatchController or AiMatchService.
/// </summary>
public interface IMatchingStrategy
{
    MatchingMode Mode { get; }
    Task<MatchingResult> MatchAsync(MatchingInput input, CancellationToken cancellationToken = default);
}
