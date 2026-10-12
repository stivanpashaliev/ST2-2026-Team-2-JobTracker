
using Microsoft.EntityFrameworkCore;
using ST2_2026_Team_2_JobTracker.Data;
using ST2_2026_Team_2_JobTracker.Models.Entities;
using ST2_2026_Team_2_JobTracker.Models.Enums;
using ST2_2026_Team_2_JobTracker.Services.Interfaces;

namespace ST2_2026_Team_2_JobTracker.Services;

public class JobOfferService : IJobOfferService
{
    private readonly ApplicationDbContext _context;

    public JobOfferService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(JobOffer jobOffer)
    {
        _context.JobOffers.Add(jobOffer);
        await _context.SaveChangesAsync();
    }

    public async Task<List<JobOffer>> GetAllAsync()
    {
        return await _context.JobOffers
            .AsNoTracking()
            .OrderBy(j => j.Title)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.JobOffers
            .AnyAsync(j => j.Id == id);
    }

    public async Task<bool> HasApplicationAsync(int id)
    {
        return await _context.Applications
            .AnyAsync(a => a.JobOfferId == id);
    }


    public async Task<List<JobOffer>> GetFilteredAsync(
        string? title,
        int? companyId,
        string? location,
        RemoteType? remoteType)
    {
        IQueryable<JobOffer> query = _context.JobOffers
            .AsNoTracking()
            .Include(j => j.Company);

        if (!string.IsNullOrWhiteSpace(title))
        {
            query = query.Where(j =>
                j.Title.Contains(title.Trim()));
        }

        if (companyId.HasValue)
        {
            query = query.Where(j =>
                j.CompanyId == companyId.Value);
        }

        if (!string.IsNullOrWhiteSpace(location))
        {
            query = query.Where(j =>
                j.Location != null &&
                j.Location.Contains(location.Trim()));
        }

        if (remoteType.HasValue)
        {
            query = query.Where(j =>
                j.RemoteType == remoteType.Value);
        }

        return await query
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync();
    }
}