using System.Diagnostics;
using Jobalatica.Models;
using Jobalatica.Models.ViewModels;
using Jobalatica.Models.Entities;
using Jobalatica.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Jobalatica.Controllers
{
    public class HomeController : Controller
    {
        private readonly IRankingService _rankingService;
        private readonly IJobService _jobService;
        private readonly IRecommendationService _recommendationService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            IRankingService rankingService,
            IJobService jobService,
            IRecommendationService recommendationService,
            UserManager<ApplicationUser> userManager,
            ILogger<HomeController> logger)
        {
            _rankingService = rankingService;
            _jobService = jobService;
            _recommendationService = recommendationService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var topRoles = await _rankingService.GetTopRolesAsync(10);
            var topSkills = await _rankingService.GetTopSkillsAsync(10);
            var recentJobs = await _jobService.GetRecentAsync(6);

            List<Job>? personalizedJobs = null;
            bool showSalaryPopup = false;

            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = _userManager.GetUserId(User);
                if (userId != null)
                {
                    personalizedJobs = await _recommendationService.GetRecommendedJobsAsync(userId, 6);
                    
                    // NEW: Check if user has contributed salary
                    var hasContributed = await _rankingService.GetSalaryRangeAsync("ANY_TITLE"); // Logic: we need to check if ANY report exists
                    // Better logic: use context directly for performance here
                    var reportsCount = await _userManager.Users
                        .Where(u => u.Id == userId)
                        .SelectMany(u => u.SalaryReports)
                        .CountAsync();
                    
                    if (reportsCount == 0)
                    {
                        showSalaryPopup = true;
                    }
                }
            }

            var vm = new HomeViewModel
            {
                TopRoles = topRoles,
                TopSkills = topSkills,
                RecentJobs = recentJobs,
                PersonalizedJobs = personalizedJobs
            };

            ViewBag.ShowSalaryPopup = showSalaryPopup;

            return View(vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
