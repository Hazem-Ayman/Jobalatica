using Jobalatica.Models.Entities;

namespace Jobalatica.Services
{
    public interface IRecommendationService
    {
        Task<List<Job>> GetRecommendedJobsAsync(string userId, int count = 10);
    }
}
