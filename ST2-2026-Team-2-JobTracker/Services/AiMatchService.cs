using Microsoft.EntityFrameworkCore;
using ST2_2026_Team_2_JobTracker.Data;
using ST2_2026_Team_2_JobTracker.DesignPatterns.Factory;
using ST2_2026_Team_2_JobTracker.Models.AI;
using ST2_2026_Team_2_JobTracker.Models.Enums;
using ST2_2026_Team_2_JobTracker.Services.Interfaces;

namespace ST2_2026_Team_2_JobTracker.Services;

public sealed class AiMatchService(
    ApplicationDbContext context,
    MatchingStrategyFactory strategyFactory) : IAiMatchService
{
    public async Task<IReadOnlyList<AiMatchOfferOption>> GetJobOffersAsync(
        CancellationToken cancellationToken = default) =>
        await context.JobOffers.AsNoTracking()
            .OrderBy(offer => offer.Title).ThenBy(offer => offer.Id)
            .Select(offer => new AiMatchOfferOption(
                offer.Id, offer.Title, offer.Company.Name))
            .ToListAsync(cancellationToken);

    public async Task<MatchingResult?> AnalyzeAsync(
        int jobOfferId, string profile, MatchingMode mode,
        CancellationToken cancellationToken = default)
    {
        var offer = await context.JobOffers.AsNoTracking()
            .Where(o => o.Id == jobOfferId)
            .Select(o => new { o.Title, o.Description, CompanyName = o.Company.Name })
            .SingleOrDefaultAsync(cancellationToken);

        if (offer is null)
            return null;

        var input = new MatchingInput(offer.Title, offer.Description, offer.CompanyName, profile);
        return await strategyFactory.Create(mode).MatchAsync(input, cancellationToken);
    }
}
