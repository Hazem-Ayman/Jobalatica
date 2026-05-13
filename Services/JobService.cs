using Jobalatica.Data;
using Jobalatica.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Jobalatica.Services
{
    public class JobService : IJobService
    {
        private readonly ApplicationDbContext _db;

        public JobService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<(List<Job> Jobs, int TotalCount)> SearchAsync(
            string? query,
            string? location,
            string? experienceLevel,
            decimal? salaryMin,
            decimal? salaryMax,
            List<int>? skillIds,
            int page = 1,
            int pageSize = 20)
        {
            var jobsQuery = _db.Jobs
                .Include(job => job.JobSkills)
                    .ThenInclude(jobSkill => jobSkill.Skill)
                .Where(job => job.IsActive);

            if (!string.IsNullOrWhiteSpace(query))
            {
                var words = query.Trim().ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                foreach (var word in words)
                {
                    jobsQuery = jobsQuery.Where(job =>
                        job.Title.ToLower().Contains(word) ||
                        job.Company.ToLower().Contains(word) ||
                        job.JobSkills.Any(js => js.Skill.Name.ToLower().Contains(word)));
                }
            }

            if (!string.IsNullOrWhiteSpace(location))
            {
                jobsQuery = jobsQuery.Where(job => job.Location == location);
            }

            if (!string.IsNullOrWhiteSpace(experienceLevel))
            {
                jobsQuery = jobsQuery.Where(job => job.ExperienceLevel == experienceLevel);
            }

            if (salaryMin.HasValue)
            {
                jobsQuery = jobsQuery.Where(job => job.SalaryMax == null || job.SalaryMax >= salaryMin.Value);
            }

            if (salaryMax.HasValue)
            {
                jobsQuery = jobsQuery.Where(job => job.SalaryMin == null || job.SalaryMin <= salaryMax.Value);
            }

            if (skillIds is { Count: > 0 })
            {
                jobsQuery = jobsQuery.Where(job =>
                    job.JobSkills.Any(jobSkill => skillIds.Contains(jobSkill.SkillId)));
            }

            page = Math.Max(page, 1);
            pageSize = Math.Max(pageSize, 1);

            var totalCount = await jobsQuery.CountAsync();
            var jobs = await jobsQuery
                .OrderByDescending(job => job.PostedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (jobs, totalCount);
        }

        public Task<Job?> GetByIdAsync(long id)
        {
            return _db.Jobs
                .Include(job => job.JobSkills)
                    .ThenInclude(jobSkill => jobSkill.Skill)
                .FirstOrDefaultAsync(job => job.Id == id);
        }

        public Task<List<Job>> GetRecentAsync(int count = 10)
        {
            return _db.Jobs
                .Include(job => job.JobSkills)
                    .ThenInclude(jobSkill => jobSkill.Skill)
                .Where(job => job.IsActive)
                .OrderByDescending(job => job.PostedAt)
                .Take(count)
                .ToListAsync();
        }

        public Task<bool> IsJobSavedAsync(string userId, long jobId)
        {
            return _db.SavedJobs.AnyAsync(savedJob =>
                savedJob.UserId == userId && savedJob.JobId == jobId);
        }

        public async Task SaveJobAsync(string userId, long jobId)
        {
            if (await IsJobSavedAsync(userId, jobId))
            {
                return;
            }

            _db.SavedJobs.Add(new SavedJob
            {
                UserId = userId,
                JobId = jobId,
                SavedAt = DateTime.UtcNow
            });

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Another request may have saved the same job first. The unique index keeps the data correct.
            }
        }

        public async Task UnsaveJobAsync(string userId, long jobId)
        {
            var savedJob = await _db.SavedJobs.FirstOrDefaultAsync(item =>
                item.UserId == userId && item.JobId == jobId);

            if (savedJob == null)
            {
                return;
            }

            _db.SavedJobs.Remove(savedJob);
            await _db.SaveChangesAsync();
        }
        
        public Task<List<long>> GetSavedJobIdsAsync(string userId)
        {
            return _db.SavedJobs
                .Where(sj => sj.UserId == userId)
                .Select(sj => sj.JobId)
                .ToListAsync();
        }
    }
}
