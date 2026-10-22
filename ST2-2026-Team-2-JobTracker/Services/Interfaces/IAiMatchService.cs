using ST2_2026_Team_2_JobTracker.Models.AI;
using ST2_2026_Team_2_JobTracker.Models.Enums;

namespace ST2_2026_Team_2_JobTracker.Services.Interfaces;

public interface IAiMatchService
{
    Task<IReadOnlyList<AiMatchOfferOption>> GetJobOffersAsync(
        CancellationToken cancellationToken = default);

    // Null means the selected job offer does not exist.
    Task<MatchingResult?> AnalyzeAsync(int jobOfferId, string profile, MatchingMode mode,
        CancellationToken cancellationToken = default);
}

public sealed record AiMatchOfferOption(int Id, string Title, string CompanyName);
