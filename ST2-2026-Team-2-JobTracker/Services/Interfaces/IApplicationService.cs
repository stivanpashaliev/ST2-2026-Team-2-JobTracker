using ST2_2026_Team_2_JobTracker.Models.Entities;

namespace ST2_2026_Team_2_JobTracker.Services.Interfaces;

public interface IApplicationService
{
    Task CreateAsync(Application application);
}