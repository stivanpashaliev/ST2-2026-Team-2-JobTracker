
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

    // Single correlated NOT EXISTS query, instead of one query per offer (N+1).
    // On Edit, keep the offer currently assigned to this application selectable.
    public Task<List<JobOffer>> GetAvailableForApplicationAsync(int? currentJobOfferId = null)
    {
        return _context.JobOffers
            .AsNoTracking()
            .Where(j => j.Id == currentJobOfferId ||
                !_context.Applications.Any(a => a.JobOfferId == j.Id))
            .OrderBy(j => j.Title)
            .ThenBy(j => j.Id)
            .ToListAsync();
    }

    public async Task<(List<JobOffer> Items, int TotalCount)> GetFilteredAsync(
        string? title,
        int? companyId,
        string? location,
        RemoteType? remoteType,
        decimal? minSalary,
        decimal? maxSalary,
        int page,
        int pageSize)
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

        // Filter by minimum salary
        if (minSalary.HasValue)
        {
            query = query.Where(j =>
                j.Salary.HasValue &&
                j.Salary.Value >= minSalary.Value);
        }

        // Filter by maximum salary
        if (maxSalary.HasValue)
        {
            query = query.Where(j =>
                j.Salary.HasValue &&
                j.Salary.Value <= maxSalary.Value);
        }

        var totalCount = await query.CountAsync();

        // Clamp before Skip: (int.MaxValue - 1) * pageSize can overflow int.
        var lastPage = Math.Max(1, (int)Math.Ceiling((double)totalCount / pageSize));
        page = Math.Clamp(page, 1, lastPage);

        var items = await query
            .OrderByDescending(j => j.CreatedAt)
            .ThenBy(j => j.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<JobOffer?> GetByIdAsync(int id)
    {
        return await _context.JobOffers
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == id);
    }

    public async Task<bool> UpdateAsync(JobOffer jobOffer)
    {
        var existing = await _context.JobOffers.FindAsync(jobOffer.Id);
        if (existing == null)
        {
            return false;
        }

        existing.CompanyId = jobOffer.CompanyId;
        existing.Title = jobOffer.Title;
        existing.Description = jobOffer.Description;
        existing.Location = jobOffer.Location;
        existing.Salary = jobOffer.Salary;
        existing.RemoteType = jobOffer.RemoteType;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<DeleteResult> DeleteAsync(int id)
    {
        var existing = await _context.JobOffers.FindAsync(id);
        if (existing == null)
        {
            return DeleteResult.NotFound;
        }

        if (await HasApplicationAsync(id))
        {
            return DeleteResult.HasDependencies;
        }

        _context.JobOffers.Remove(existing);
        await _context.SaveChangesAsync();
        return DeleteResult.Deleted;
    }
}
