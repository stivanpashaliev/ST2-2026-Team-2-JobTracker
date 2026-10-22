
using Microsoft.EntityFrameworkCore;
using ST2_2026_Team_2_JobTracker.Data;
using ST2_2026_Team_2_JobTracker.Models.Entities;
using ST2_2026_Team_2_JobTracker.Models.Enums;
using ST2_2026_Team_2_JobTracker.Services.Interfaces;

namespace ST2_2026_Team_2_JobTracker.Services;

public class ApplicationService : IApplicationService
{
    private readonly ApplicationDbContext _context;

    public ApplicationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(Application application)
    {
        _context.Applications.Add(application);
        await _context.SaveChangesAsync();
    }



    public async Task<(List<Application> Items, int TotalCount)> GetFilteredAsync(
        string? jobTitle,
        int? companyId,
        ApplicationStatus? status,
        int page,
        int pageSize)
    {
        IQueryable<Application> query = _context.Applications
            .AsNoTracking()
            .Include(a => a.JobOffer)
            .ThenInclude(j => j.Company);

        if (!string.IsNullOrWhiteSpace(jobTitle))
        {
            query = query.Where(a =>
                a.JobOffer.Title.Contains(jobTitle.Trim()));
        }

        if (companyId.HasValue)
        {
            query = query.Where(a =>
                a.JobOffer.CompanyId == companyId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(a =>
                a.Status == status.Value);
        }

        var totalCount = await query.CountAsync();

        // Clamp before Skip: (int.MaxValue - 1) * pageSize can overflow int.
        var lastPage = Math.Max(1, (int)Math.Ceiling((double)totalCount / pageSize));
        page = Math.Clamp(page, 1, lastPage);

        var items = await query
            .OrderByDescending(a => a.AppliedAt)
            .ThenBy(a => a.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Application?> GetByIdAsync(int id)
    {
        return await _context.Applications
            .AsNoTracking()
            .Include(a => a.JobOffer)
            .SingleOrDefaultAsync(item => item.Id == id);
    }

    public async Task<bool> UpdateAsync(Application application)
    {
        var existing = await _context.Applications.FindAsync(application.Id);
        if (existing == null)
        {
            return false;
        }

        existing.JobOfferId = application.JobOfferId;
        existing.Status = application.Status;
        existing.AppliedAt = application.AppliedAt;
        existing.Notes = application.Notes;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<DeleteResult> DeleteAsync(int id)
    {
        var existing = await _context.Applications.FindAsync(id);
        if (existing == null)
        {
            return DeleteResult.NotFound;
        }

        _context.Applications.Remove(existing);
        await _context.SaveChangesAsync();
        return DeleteResult.Deleted;
    }
}