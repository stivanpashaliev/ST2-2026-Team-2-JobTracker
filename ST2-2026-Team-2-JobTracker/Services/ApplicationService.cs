
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

        var items = await query
            .OrderByDescending(a => a.AppliedAt)
            .ThenBy(a => a.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}