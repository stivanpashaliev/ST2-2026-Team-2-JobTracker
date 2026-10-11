using ST2_2026_Team_2_JobTracker.Models.Entities;

namespace ST2_2026_Team_2_JobTracker.Services.Interfaces;

public interface IJobOfferService
{
    Task CreateAsync(JobOffer jobOffer);
    Task<List<JobOffer>> GetAllAsync();
    Task<bool> ExistsAsync(int id);
    Task<bool> HasApplicationAsync(int id);
}