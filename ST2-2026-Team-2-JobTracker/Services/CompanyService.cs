
using Microsoft.EntityFrameworkCore;
using ST2_2026_Team_2_JobTracker.Data;
using ST2_2026_Team_2_JobTracker.Models.Entities;
using ST2_2026_Team_2_JobTracker.Services.Interfaces;

namespace ST2_2026_Team_2_JobTracker.Services;

public class CompanyService : ICompanyService
{
    private readonly ApplicationDbContext _context;

    public CompanyService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(Company company)
    {
        _context.Companies.Add(company);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Company>> GetAllAsync()
    {
        return await _context.Companies
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Companies
            .AnyAsync(c => c.Id == id);
    }


    public async Task<List<Company>> GetFilteredAsync(
        string? name,
        string? location)
    {
        IQueryable<Company> query = _context.Companies
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(c => c.Name.Contains(name.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(location))
        {
            query = query.Where(c =>
                c.Location != null &&
                c.Location.Contains(location.Trim()));
        }

        return await query
            .OrderBy(c => c.Name)
            .ToListAsync();
    }
}
