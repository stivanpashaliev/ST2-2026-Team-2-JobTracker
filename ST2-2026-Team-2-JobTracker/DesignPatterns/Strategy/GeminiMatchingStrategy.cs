using ST2_2026_Team_2_JobTracker.Models.AI;
using ST2_2026_Team_2_JobTracker.Models.Enums;
using ST2_2026_Team_2_JobTracker.Services.AI;

namespace ST2_2026_Team_2_JobTracker.DesignPatterns.Strategy;

/// <summary>STRATEGY implementation: semantic evaluation by Google's Gemini LLM.</summary>
public sealed class GeminiMatchingStrategy(IGeminiClient geminiClient) : IMatchingStrategy
{
    public MatchingMode Mode => MatchingMode.Gemini;

    public Task<MatchingResult> MatchAsync(
        MatchingInput input, CancellationToken cancellationToken = default) =>
        geminiClient.AnalyzeAsync(input, cancellationToken);
}
