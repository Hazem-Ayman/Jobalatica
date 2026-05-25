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

        public async Task<(List<Skill> Matching, List<Skill> Missing, int Percentage)> GetSkillGapAsync(string userId, long jobId)
        {
            var userSkillIds = await _db.UserSkills
                .Where(us => us.UserId == userId)
                .Select(us => us.SkillId)
                .ToListAsync();

            var jobSkills = await _db.JobSkills
                .Include(js => js.Skill)
                .Where(js => js.JobId == jobId)
                .Select(js => js.Skill)
                .ToListAsync();

            if (!jobSkills.Any()) return (new List<Skill>(), new List<Skill>(), 0);

            var matching = jobSkills.Where(s => userSkillIds.Contains(s.Id)).ToList();
            var missing = jobSkills.Where(s => !userSkillIds.Contains(s.Id)).ToList();
            
            var percentage = (int)((double)matching.Count / jobSkills.Count * 100);

            return (matching, missing, percentage);
        }
    }
}
