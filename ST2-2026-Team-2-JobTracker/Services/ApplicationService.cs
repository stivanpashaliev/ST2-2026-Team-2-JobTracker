
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


    public async Task<List<Application>> GetFilteredAsync(
        string? jobTitle,
        int? companyId,
        ApplicationStatus? status)
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

        return await query
            .OrderByDescending(a => a.AppliedAt)
            .ThenByDescending(a => a.Id)
            .ToListAsync();
    }
}
