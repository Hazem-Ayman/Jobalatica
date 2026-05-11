using Jobalatica.Models.Entities;

namespace Jobalatica.Services
{
    public interface IJobService
    {
        Task<(List<Job> Jobs, int TotalCount)> SearchAsync(
            string? query,
            string? location,
            string? experienceLevel,
            decimal? salaryMin,
            decimal? salaryMax,
            List<int>? skillIds,
            int page = 1,
            int pageSize = 20);

        Task<Job?> GetByIdAsync(long id);
        Task<List<Job>> GetRecentAsync(int count = 10);
        Task<bool> IsJobSavedAsync(string userId, long jobId);
        Task SaveJobAsync(string userId, long jobId);
        Task UnsaveJobAsync(string userId, long jobId);
        Task<List<long>> GetSavedJobIdsAsync(string userId);
    }
}
