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
        public async Task<IActionResult> Index()
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
        public async Task<IActionResult> RoleDetail(string title)
        {
            if (string.IsNullOrEmpty(title)) return RedirectToAction("Index");

            var trend = await _rankingService.GetRoleTrendAsync(title);
            var salaryInfo = await _rankingService.GetSalaryRangeAsync(title);
            var (sampleJobs, _) = await _jobService.SearchAsync(title, null, null, null, null, null, 1, 6);

            bool hasContributed = false;
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = _userManager.GetUserId(User);
                hasContributed = await _context.SalaryReports.AnyAsync(r => r.UserId == userId);
            }

            var vm = new RoleDetailViewModel
            {
                JobTitle = title,
                AvgSalaryMin = salaryInfo.AvgMin,
                AvgSalaryMax = salaryInfo.AvgMax,
                SampleSize = salaryInfo.SampleSize,
                Trend = trend,
                SampleJobs = sampleJobs,
                CommonSkills = (await _rankingService.GetTopSkillsAsync(8)),
                HasContributedSalary = hasContributed
            };

            return View(vm);
        }
    }
}
