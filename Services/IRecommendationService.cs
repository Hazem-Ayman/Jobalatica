using Jobalatica.Models.Entities;

namespace Jobalatica.Services
{
    public interface IRecommendationService
    {
        Task<List<Job>> GetRecommendedJobsAsync(string userId, int count = 10);
        Task<(List<Skill> Matching, List<Skill> Missing, int Percentage)> GetSkillGapAsync(string userId, long jobId);
    }
}
