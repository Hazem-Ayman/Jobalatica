using Jobalatica.Data;
using Jobalatica.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Jobalatica.Services
{
    public class RecommendationService : IRecommendationService
    {
        private readonly ApplicationDbContext _db;

        public RecommendationService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<Job>> GetRecommendedJobsAsync(string userId, int count = 10)
        {
            var userSkillIds = await _db.UserSkills
                .Where(userSkill => userSkill.UserId == userId)
                .Select(userSkill => userSkill.SkillId)
                .ToListAsync();

            if (userSkillIds.Count == 0)
            {
                return new List<Job>();
            }

            var jobIds = await _db.JobSkills
                .Where(jobSkill => userSkillIds.Contains(jobSkill.SkillId))
                .GroupBy(jobSkill => jobSkill.JobId)
                .OrderByDescending(group => group.Count())
                .Take(count)
                .Select(group => group.Key)
                .ToListAsync();

            return await _db.Jobs
                .Include(job => job.JobSkills)
                    .ThenInclude(jobSkill => jobSkill.Skill)
                .Where(job => job.IsActive && jobIds.Contains(job.Id))
                .ToListAsync();
        }
    }
}
