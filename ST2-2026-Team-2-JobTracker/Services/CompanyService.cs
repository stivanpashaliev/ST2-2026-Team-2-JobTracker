
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



    public async Task<(List<Company> Items, int TotalCount)> GetFilteredAsync(
        string? name,
        string? location,
        int page,
        int pageSize)
    {
        IQueryable<Company> query = _context.Companies
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(c =>
                c.Name.Contains(name.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(location))
        {
            query = query.Where(c =>
                c.Location != null &&
                c.Location.Contains(location.Trim()));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(c => c.Name)
            .ThenBy(c => c.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }


    public async Task<Company?> GetByIdAsync(int id)
    {
        return await _context.Companies
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == id);
    }

    public async Task<bool> UpdateAsync(Company company)
    {
        var existing = await _context.Companies.FindAsync(company.Id);
        if (existing == null)
        {
            return false;
        }

        existing.Name = company.Name;
        existing.Website = company.Website;
        existing.Location = company.Location;

        await _context.SaveChangesAsync();
        return true;
    }

    public Task<bool> HasJobOffersAsync(int id)
    {
        return _context.JobOffers.AnyAsync(j => j.CompanyId == id);
    }

    public async Task<DeleteResult> DeleteAsync(int id)
    {
        var existing = await _context.Companies.FindAsync(id);
        if (existing == null)
        {
            return DeleteResult.NotFound;
        }

        if (await HasJobOffersAsync(id))
        {
            return DeleteResult.HasDependencies;
        }

        _context.Companies.Remove(existing);
        await _context.SaveChangesAsync();
        return DeleteResult.Deleted;
    }
}
