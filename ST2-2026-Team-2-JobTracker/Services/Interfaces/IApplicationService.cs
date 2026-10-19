using ST2_2026_Team_2_JobTracker.Models.Entities;
using ST2_2026_Team_2_JobTracker.Models.Enums;

using ST2_2026_Team_2_JobTracker.Services;

namespace ST2_2026_Team_2_JobTracker.Services.Interfaces;

public interface IApplicationService
{
    Task<Application?> GetByIdAsync(int id);
    Task<bool> UpdateAsync(Application application);
    Task<DeleteResult> DeleteAsync(int id);

    Task CreateAsync(Application application);
    Task<(List<Application> Items, int TotalCount)> GetFilteredAsync(
        string? jobTitle,
        int? companyId,
        ApplicationStatus? status,
        int page,
        int pageSize);
}