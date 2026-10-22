using ST2_2026_Team_2_JobTracker.Models.AI;

namespace ST2_2026_Team_2_JobTracker.Services.AI;

public interface IGeminiClient
{
    Task<MatchingResult> AnalyzeAsync(MatchingInput input, CancellationToken cancellationToken = default);
}
