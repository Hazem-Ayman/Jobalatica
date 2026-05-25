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
                .Where(job => job.IsActive && !job.Title.Contains("Confidential"))
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

        public async Task<int> CalculateDemandScoreAsync(string title)
        {
            var totalJobs = await _db.Jobs.CountAsync(j => j.IsActive);
            if (totalJobs == 0) return 0;

            var roleJobs = await _db.Jobs.CountAsync(j => j.IsActive && j.Title.Contains(title));
            var avgSalary = await _db.Jobs.Where(j => j.IsActive).AverageAsync(j => (double?)((j.SalaryMin + j.SalaryMax) / 2)) ?? 50000;
            var roleSalary = await _db.Jobs.Where(j => j.IsActive && j.Title.Contains(title)).AverageAsync(j => (double?)((j.SalaryMin + j.SalaryMax) / 2)) ?? (double)avgSalary;

            // Weight 1: Volume (relative to other roles)
            double volumeWeight = (double)roleJobs / (totalJobs / 10.0); // Normalizing against top 10 average
            
            // Weight 2: Value (salary premium)
            double salaryWeight = roleSalary / avgSalary;

            int score = (int)((volumeWeight * 50) + (salaryWeight * 50));
            return Math.Clamp(score, 15, 100);
        }

        public async Task<List<(string SkillName, int Percentage)>> GetSkillDistributionAsync(string title)
        {
            var totalRoleJobs = await _db.Jobs.CountAsync(j => j.IsActive && j.Title.Contains(title));
            if (totalRoleJobs == 0) return new List<(string, int)>();

            return await _db.JobSkills
                .Include(js => js.Skill)
                .Where(js => js.Job.Title.Contains(title))
                .GroupBy(js => js.Skill.Name)
                .OrderByDescending(g => g.Count())
                .Take(6)
                .Select(g => new ValueTuple<string, int>(g.Key, (int)((double)g.Count() / totalRoleJobs * 100)))
                .ToListAsync();
        }

        public async Task<Jobalatica.Models.ViewModels.RoleHealth> GetRoleHealthAsync(string title)
        {
            var roleJobs = await _db.Jobs.Where(j => j.IsActive && j.Title.Contains(title)).ToListAsync();
            var totalJobs = await _db.Jobs.CountAsync(j => j.IsActive);
            
            if (!roleJobs.Any()) return new Jobalatica.Models.ViewModels.RoleHealth { Score = 0, Status = "Unknown" };

            // 1. Diversity Score (How many different employers?)
            var uniqueCompanies = roleJobs.Select(j => j.Company).Distinct().Count();
            // Logical: 1 company = 10%, 5+ companies = 100% (relative to role size)
            double diversityRatio = (double)uniqueCompanies / Math.Min(roleJobs.Count, 8);
            int diversityScore = Math.Clamp((int)(diversityRatio * 100), 5, 100);

            // 2. Pipeline Score (Is the role accessible to new talent?)
            var entryCount = roleJobs.Count(j => j.ExperienceLevel == "Entry");
            // Logical: If 0 entry jobs, score is 0. If 20% are entry, score is 100.
            double pipelineRatio = (double)entryCount / (roleJobs.Count * 0.20);
            int pipelineScore = Math.Clamp((int)(pipelineRatio * 100), 0, 100);

            // 3. Value Score (Salary premium vs Market Max)
            var avgSalary = await _db.Jobs.Where(j => j.IsActive).AverageAsync(j => (double?)((j.SalaryMin + j.SalaryMax) / 2)) ?? 50000;
            var maxRoleAvg = await _db.Jobs.Where(j => j.IsActive).GroupBy(j => j.Title).Select(g => g.Average(j => (double?)((j.SalaryMin + j.SalaryMax) / 2))).MaxAsync() ?? 150000;
            var roleAvg = roleJobs.Average(j => (double?)((j.SalaryMin + j.SalaryMax) / 2)) ?? avgSalary;
            
            // Logical: Compare role average against the gap between global average and global max
            int valueScore = Math.Clamp((int)((roleAvg - (avgSalary * 0.6)) / (maxRoleAvg - (avgSalary * 0.6)) * 100), 10, 100);

            int finalScore = (int)((diversityScore * 0.4) + (pipelineScore * 0.3) + (valueScore * 0.3));

            var health = new Jobalatica.Models.ViewModels.RoleHealth
            {
                Score = finalScore,
                DiversityScore = diversityScore,
                PipelineScore = pipelineScore,
                ValueScore = valueScore
            };

            if (finalScore > 70) { health.Status = "Robust"; health.StatusColor = "#4ADE80"; health.Recommendation = "High job security. Market is diverse and entry-friendly."; }
            else if (finalScore > 40) { health.Status = "Stable"; health.StatusColor = "#FACC15"; health.Recommendation = "Standard market conditions. Expect moderate competition."; }
            else { health.Status = "Vulnerable"; health.StatusColor = "#F87171"; health.Recommendation = "Market concentration is high. Narrow path for new entrants."; }

            return health;
        }

        public async Task<List<DemandSnapshot>> GetRoleTrendAsync(string title, string? location = null)
        {
            var snapshots = await _db.DemandSnapshots
                .Where(snapshot => snapshot.JobTitle == title)
                .OrderBy(snapshot => snapshot.SnapshotDate)
                .ToListAsync();

            // If no exact match, try to find a trend for the base role (e.g. "Senior Backend Engineer" -> "Backend Engineer")
            if (!snapshots.Any())
            {
                var roles = new[] { "Backend Engineer", "Frontend Engineer", "Full Stack Developer", "Data Scientist", "DevOps Engineer", "Mobile Developer", "Product Manager", "UI/UX Designer", "Security Analyst", "QA Engineer", "Machine Learning Engineer" };
                var baseRole = roles.FirstOrDefault(r => title.Contains(r));
                if (baseRole != null)
                {
                    snapshots = await _db.DemandSnapshots
                        .Where(snapshot => snapshot.JobTitle == baseRole)
                        .OrderBy(snapshot => snapshot.SnapshotDate)
                        .ToListAsync();
                }
            }

            return snapshots;
        }

        public async Task<List<(string Level, decimal AvgMin, decimal AvgMax)>> GetSalaryByExperienceAsync(string title)
        {
            var experienceLevels = new[] { "Entry", "Mid", "Senior", "Lead", "Director" };
            var results = new List<(string Level, decimal AvgMin, decimal AvgMax)>();

            var avgSalary = await _db.Jobs.Where(j => j.IsActive).AverageAsync(j => (double?)((j.SalaryMin + j.SalaryMax) / 2)) ?? 50000;

            // Identify the base role to get a broader salary matrix
            var roles = new[] { "Backend Engineer", "Frontend Engineer", "Full Stack Developer", "Data Scientist", "DevOps Engineer", "Mobile Developer", "Product Manager", "UI/UX Designer", "Security Analyst", "QA Engineer", "Machine Learning Engineer", "Data Analyst" };
            var baseRole = roles.FirstOrDefault(r => title.Contains(r)) ?? title;

            // If the title is "Senior [Role]", "Entry [Role]", etc., try to strip that part
            var cleanTitle = title.Replace("Senior ", "").Replace("Junior ", "").Replace("Lead ", "").Replace("Entry ", "").Replace("Mid ", "").Trim();

            foreach (var level in experienceLevels)
            {
                var stats = await _db.Jobs
                    .Where(j => (j.Title.Contains(baseRole) || j.Title.Contains(cleanTitle)) && j.ExperienceLevel == level && j.SalaryMin.HasValue && j.SalaryMax.HasValue)
                    .Select(j => new { Min = (double)j.SalaryMin!.Value, Max = (double)j.SalaryMax!.Value })
                    .ToListAsync();

                if (stats.Any())
                {
                    results.Add((level, (decimal)stats.Average(s => s.Min), (decimal)stats.Average(s => s.Max)));
                }
                else
                {
                    // Logical Fallback: If no specific data for this level, use the role average or global average with experience multipliers
                    decimal multiplier = level switch { "Entry" => 0.65m, "Mid" => 1.0m, "Senior" => 1.4m, "Lead" => 1.8m, "Director" => 2.2m, _ => 1.0m };
                    var baseAvgMin = (decimal)avgSalary * 0.8m;
                    var baseAvgMax = (decimal)avgSalary * 1.2m;
                    
                    results.Add((level, baseAvgMin * multiplier, baseAvgMax * multiplier));
                }
            }

            return results;
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
