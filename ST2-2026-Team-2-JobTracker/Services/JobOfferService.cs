
using Microsoft.EntityFrameworkCore;
using ST2_2026_Team_2_JobTracker.Data;
using ST2_2026_Team_2_JobTracker.Models.Entities;
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
}
