using ST2_2026_Team_2_JobTracker.Models.Entities;

namespace ST2_2026_Team_2_JobTracker.Services.Interfaces;

public interface ICompanyService
{
    Task CreateAsync(Company company);
    Task<List<Company>> GetAllAsync();
    Task<bool> ExistsAsync(int id);
    Task<(List<Company> Items, int TotalCount)> GetFilteredAsync(
    string? name,
    string? location,
    int page,
    int pageSize);
}