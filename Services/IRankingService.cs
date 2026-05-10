using Jobalatica.Models.Entities;

namespace Jobalatica.Services
{
    public interface IRankingService
    {
        Task<List<(string Title, int Count)>> GetTopRolesAsync(int count = 10);
        Task<List<Skill>> GetTopSkillsAsync(int count = 15);
        Task<List<DemandSnapshot>> GetRoleTrendAsync(string title, string? location = null);
        Task<(decimal AvgMin, decimal AvgMax, int SampleSize)> GetSalaryRangeAsync(string title, string? location = null);
    }
}
