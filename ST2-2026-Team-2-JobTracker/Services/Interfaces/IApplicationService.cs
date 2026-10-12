using ST2_2026_Team_2_JobTracker.Models.Entities;
using ST2_2026_Team_2_JobTracker.Models.Enums;

namespace ST2_2026_Team_2_JobTracker.Services.Interfaces;

public interface IApplicationService
{
    Task CreateAsync(Application application);
    Task<List<Application>> GetFilteredAsync(
    string? jobTitle,
    int? companyId,
    ApplicationStatus? status);
}