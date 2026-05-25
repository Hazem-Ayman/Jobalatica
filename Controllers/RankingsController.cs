using Jobalatica.Models.ViewModels;
using Jobalatica.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Jobalatica.Controllers
{
    public class RankingsController : Controller
    {
        private readonly IRankingService _rankingService;
        private readonly IJobService _jobService;
        private readonly Data.ApplicationDbContext _context;
        private readonly Microsoft.AspNetCore.Identity.UserManager<Models.Entities.ApplicationUser> _userManager;

        public RankingsController(
            IRankingService rankingService, 
            IJobService jobService, 
            Data.ApplicationDbContext context,
            Microsoft.AspNetCore.Identity.UserManager<Models.Entities.ApplicationUser> userManager)
        {
            _rankingService = rankingService;
            _jobService = jobService;
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index() // Shows market trends overview
        {
            var topRoles = await _rankingService.GetTopRolesAsync(15);
            var topSkills = await _rankingService.GetTopSkillsAsync(15);

            var vm = new RankingsViewModel
            {
                TopRoles = topRoles,
                TopSkills = topSkills
            };

            return View(vm);
        }

        [HttpGet]
        public IActionResult Search(string title) // Redirects to role analysis
        {
            if (string.IsNullOrWhiteSpace(title)) return RedirectToAction("Index");
            return RedirectToAction("RoleDetail", new { title = title.Trim() });
        }

        [HttpGet]
        public async Task<IActionResult> Autocomplete(string term) // Provides role title suggestions
        {
            if (string.IsNullOrWhiteSpace(term)) return Json(new List<string>());

            var suggestions = await _context.Jobs
                .Where(j => j.Title.ToLower().Contains(term.ToLower()))
                .Select(j => j.Title)
                .Distinct()
                .Take(10)
                .ToListAsync();

            return Json(suggestions);
        }

        [HttpGet]
        public async Task<IActionResult> RoleDetail(string title) // Deep analysis of specific role
        {
            if (string.IsNullOrEmpty(title)) return RedirectToAction("Index");

            var trend = await _rankingService.GetRoleTrendAsync(title);
            var salaryInfo = await _rankingService.GetSalaryRangeAsync(title);
            var (sampleJobs, _) = await _jobService.SearchAsync(title, null, null, null, null, null, 1, 6);

            // Calculate Demand Score, Skill Distribution, and Health dynamically
            int demandScore = await _rankingService.CalculateDemandScoreAsync(title);
            var skillDistribution = await _rankingService.GetSkillDistributionAsync(title);
            var roleHealth = await _rankingService.GetRoleHealthAsync(title);

            bool hasContributed = false;
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = _userManager.GetUserId(User);
                if (userId != null)
                {
                    hasContributed = await _context.SalaryReports.AnyAsync(r => r.UserId == userId);
                }
            }

            var vm = new RoleDetailViewModel
            {
                JobTitle = title,
                AvgSalaryMin = salaryInfo.AvgMin,
                AvgSalaryMax = salaryInfo.AvgMax,
                SampleSize = salaryInfo.SampleSize,
                Trend = trend,
                SampleJobs = sampleJobs,
                HasContributedSalary = hasContributed,
                SalaryByExperience = await _rankingService.GetSalaryByExperienceAsync(title),
                DemandScore = demandScore,
                TopCompanies = await _context.Jobs
                    .Where(j => j.Title.Contains(title))
                    .GroupBy(j => j.Company)
                    .OrderByDescending(g => g.Count())
                    .Take(6)
                    .Select(g => new ValueTuple<string, int>(g.Key, g.Count()))
                    .ToListAsync(),
                SkillDistribution = skillDistribution,
                CurrentLevel = title.Contains("Director") || title.Contains("Head") || title.Contains("VP") ? "Director" :
                               title.Contains("Lead") || title.Contains("Principal") || title.Contains("Architect") ? "Lead" :
                               title.Contains("Senior") ? "Senior" : 
                               title.Contains("Entry") || title.Contains("Junior") ? "Entry" : "Mid",
                Health = roleHealth,
                CommonSkills = new List<Models.Entities.Skill>() // Start clean
            };

            // Fix "Broken" Skill Priority with Robust Static Fallbacks
            var baseRoles = new Dictionary<string, List<string>>
            {
                { "Backend", new List<string> { "SQL", "APIs", "Docker", "Redis", "C#", "System Design" } },
                { "Frontend", new List<string> { "React", "TypeScript", "CSS", "HTML5", "Next.js", "Tailwind" } },
                { "Data", new List<string> { "Python", "SQL", "Tableau", "Statistics", "PowerBI", "Pandas" } },
                { "DevOps", new List<string> { "AWS", "Terraform", "CI/CD", "Kubernetes", "Linux", "Nginx" } },
                { "Design", new List<string> { "Figma", "UI/UX", "Prototyping", "Adobe CC", "Typography", "User Research" } },
                { "Management", new List<string> { "Agile", "Roadmapping", "Stakeholders", "Budgeting", "Mentorship", "Strategy" } },
                { "Director", new List<string> { "Leadership", "Strategy", "Operations", "Finance", "Governance", "Planning" } },
                { "Engineering", new List<string> { "Architecture", "Scaling", "Code Quality", "Security", "Infrastructure", "Algorithms" } }
            };

            var matchingKey = baseRoles.Keys.FirstOrDefault(k => title.Contains(k, StringComparison.OrdinalIgnoreCase));
            if (matchingKey != null)
            {
                vm.CommonSkills = baseRoles[matchingKey].Select(s => new Models.Entities.Skill { Name = s }).ToList();
            }
            else
            {
                vm.CommonSkills = new List<Models.Entities.Skill> { 
                    new() { Name = "Strategy" }, new() { Name = "Innovation" }, 
                    new() { Name = "Leadership" }, new() { Name = "Collaboration" },
                    new() { Name = "Execution" }, new() { Name = "Communication" }
                };
            }

            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = _userManager.GetUserId(User);
                if (userId != null)
                {
                    vm.SavedJobIds = await _jobService.GetSavedJobIdsAsync(userId);
                }
            }

            return View(vm);
        }
    }
}
