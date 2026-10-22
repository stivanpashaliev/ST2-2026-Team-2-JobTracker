using Microsoft.EntityFrameworkCore;
using ST2_2026_Team_2_JobTracker.Data;
using ST2_2026_Team_2_JobTracker.Models.Enums;
using ST2_2026_Team_2_JobTracker.Models.ViewModels.Dashboard;
using ST2_2026_Team_2_JobTracker.Services.Interfaces;

namespace ST2_2026_Team_2_JobTracker.Services;

/// <summary>
/// Builds read-only statistics for the Dashboard from the current database state.
/// Aggregation and projection run on the database, not on loaded entity collections.
/// </summary>
public sealed class DashboardService : IDashboardService
{
    private const int RecentApplicationsLimit = 5;
    private readonly ApplicationDbContext _context;

    public DashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardViewModel> GetDashboardAsync(
        CancellationToken cancellationToken = default)
    {
        var companyCount = await _context.Companies
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var offerCount = await _context.JobOffers
            .AsNoTracking()
            .CountAsync(cancellationToken);

        // A JobOffer has at most one Application in the current domain model.
        // Count the truly unoccupied offers rather than subtracting unrelated counters.
        var availableOfferCount = await _context.JobOffers
            .AsNoTracking()
            .CountAsync(offer => offer.Application == null, cancellationToken);

        // SQL GROUP BY: avoids loading all applications into memory for counting.
        var statusCounts = await _context.Applications
            .AsNoTracking()
            .GroupBy(application => application.Status)
            .Select(group => new { Status = group.Key, Count = group.Count() })
            .ToListAsync(cancellationToken);

        var countsByStatus = statusCounts.ToDictionary(x => x.Status, x => x.Count);
        var allStatuses = Enum.GetValues<ApplicationStatus>()
            .Select(status => new DashboardStatusViewModel
            {
                Status = status,
                Count = countsByStatus.GetValueOrDefault(status)
            })
            .ToList();

        // Application currently has no CreatedAt field. Id DESC represents
        // most recently added records (not most recently updated records).
        // The projection causes EF Core to join JobOffers/Companies in SQL.
        var recentApplications = await _context.Applications
            .AsNoTracking()
            .OrderByDescending(application => application.Id)
            .Take(RecentApplicationsLimit)
            .Select(application => new DashboardRecentApplicationViewModel
            {
                Id = application.Id,
                JobTitle = application.JobOffer.Title,
                CompanyName = application.JobOffer.Company.Name,
                Status = application.Status,
                AppliedAt = application.AppliedAt
            })
            .ToListAsync(cancellationToken);

        return new DashboardViewModel
        {
            TotalCompanies = companyCount,
            TotalJobOffers = offerCount,
            TotalApplications = statusCounts.Sum(x => x.Count),
            AvailableJobOffers = availableOfferCount,
            ApplicationStatuses = allStatuses,
            RecentApplications = recentApplications
        };
    }
}
