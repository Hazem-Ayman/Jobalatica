using Jobalatica.Data;
using Jobalatica.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Jobalatica.Services
{
    public class RankingService : IRankingService
    {
        private readonly ApplicationDbContext _db;

        public RankingService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<(string Title, int Count)>> GetTopRolesAsync(int count = 10)
        {
            return await _db.Jobs
                .Where(job => job.IsActive)
                .GroupBy(job => job.Title)
                .OrderByDescending(group => group.Count())
                .Take(count)
                .Select(group => new ValueTuple<string, int>(group.Key, group.Count()))
                .ToListAsync();
        }

        public Task<List<Skill>> GetTopSkillsAsync(int count = 15)
        {
            return _db.Skills
                .OrderByDescending(skill => skill.TotalJobMentions)
                .Take(count)
                .ToListAsync();
        }

        public Task<List<DemandSnapshot>> GetRoleTrendAsync(string title, string? location = null)
        {
            var snapshots = _db.DemandSnapshots
                .Where(snapshot => snapshot.JobTitle == title);

            if (!string.IsNullOrWhiteSpace(location))
            {
                snapshots = snapshots.Where(snapshot => snapshot.Location == location);
            }

            return snapshots
                .OrderBy(snapshot => snapshot.SnapshotDate)
                .ToListAsync();
        }

        public async Task<(decimal AvgMin, decimal AvgMax, int SampleSize)> GetSalaryRangeAsync(string title, string? location = null)
        {
            var jobs = _db.Jobs.Where(job =>
                job.IsActive &&
                job.Title == title &&
                job.SalaryMin.HasValue &&
                job.SalaryMax.HasValue);

            var reports = _db.SalaryReports.Where(report => report.JobTitle.ToLower() == title.ToLower());

            if (!string.IsNullOrWhiteSpace(location))
            {
                jobs = jobs.Where(job => job.Location == location);
                reports = reports.Where(report => report.Location == location);
            }

            var jobSalaryRanges = await jobs
                .Select(job => new
                {
                    Min = job.SalaryMin!.Value,
                    Max = job.SalaryMax!.Value
                })
                .ToListAsync();

            var salaryReports = await reports
                .Select(report => report.Salary)
                .ToListAsync();

            var sampleSize = jobSalaryRanges.Count + salaryReports.Count;
            if (sampleSize == 0)
            {
                return (0, 0, 0);
            }

            var minTotal = jobSalaryRanges.Sum(item => item.Min) + salaryReports.Sum();
            var maxTotal = jobSalaryRanges.Sum(item => item.Max) + salaryReports.Sum();

            return (minTotal / sampleSize, maxTotal / sampleSize, sampleSize);
        }
    }
}
