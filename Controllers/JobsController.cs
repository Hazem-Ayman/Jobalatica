using Jobalatica.Models.Entities;
using Jobalatica.Models.ViewModels;
using Jobalatica.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Jobalatica.Controllers
{
    public class JobsController : Controller
    {
        private readonly IJobService _jobService;
        private readonly IRankingService _rankingService;
        private readonly IRecommendationService _recommendationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public JobsController(
            IJobService jobService,
            IRankingService rankingService,
            IRecommendationService recommendationService,
            UserManager<ApplicationUser> userManager)
        {
            _jobService = jobService;
            _rankingService = rankingService;
            _recommendationService = recommendationService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index( // Searches and filters jobs
            string? query, 
            string? location, 
            string? experienceLevel, 
            decimal? salaryMin, 
            decimal? salaryMax, 
            int page = 1)
        {
            var pageSize = 20;
            var (jobs, totalCount) = await _jobService.SearchAsync(
                query, location, experienceLevel, salaryMin, salaryMax, null, page, pageSize);

            var vm = new JobSearchViewModel
            {
                Jobs = jobs,
                TotalCount = totalCount,
                CurrentPage = page,
                PageSize = pageSize,
                Query = query,
                Location = location,
                ExperienceLevel = experienceLevel,
                SalaryMin = salaryMin,
                SalaryMax = salaryMax
            };

            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = _userManager.GetUserId(User);
                if (userId != null)
                {
                    vm.SavedJobIds = await _jobService.GetSavedJobIdsAsync(userId);
                    
                    // Only show recommendations on the first page of search results to avoid repetition
                    if (page == 1)
                    {
                        vm.RecommendedJobs = await _recommendationService.GetRecommendedJobsAsync(userId, 3);
                    }
                }
            }

            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return PartialView("_JobResults", vm);
            }

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(long id) // Shows specific job information
        {
            var job = await _jobService.GetByIdAsync(id);
            if (job == null)
            {
                return NotFound();
            }

            bool isSaved = false;
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = _userManager.GetUserId(User);
                if (userId != null)
                {
                    isSaved = await _jobService.IsJobSavedAsync(userId, id);
                }
            }

            // For similar jobs, we search for jobs with the same title or in the same location
            var (similarJobs, _) = await _jobService.SearchAsync(job.Title, null, null, null, null, null, 1, 4);

            var vm = new JobDetailViewModel
            {
                Job = job,
                IsSaved = isSaved,
                SimilarJobs = similarJobs.Where(j => j.Id != id).ToList()
            };

            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = _userManager.GetUserId(User);
                if (userId != null)
                {
                    var (matching, missing, percentage) = await _recommendationService.GetSkillGapAsync(userId, id);
                    vm.MatchingSkills = matching;
                    vm.MissingSkills = missing;
                    vm.MatchPercentage = percentage;
                }
            }

            // Sync Demand Score
            vm.DemandScore = await _rankingService.CalculateDemandScoreAsync(job.Title);

            return View(vm);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Save(long jobId) // Adds job to saved
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            await _jobService.SaveJobAsync(userId, jobId);
            return Ok();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Unsave(long jobId) // Removes job from saved
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            await _jobService.UnsaveJobAsync(userId, jobId);
            return Ok();
        }
    }
}
