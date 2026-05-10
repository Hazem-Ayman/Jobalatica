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
        private readonly UserManager<ApplicationUser> _userManager;

        public JobsController(
            IJobService jobService,
            IRankingService rankingService,
            UserManager<ApplicationUser> userManager)
        {
            _jobService = jobService;
            _rankingService = rankingService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
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

            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return PartialView("_JobResults", vm);
            }

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(long id)
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

            return View(vm);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Save(long jobId)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            await _jobService.SaveJobAsync(userId, jobId);
            return Ok();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Unsave(long jobId)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            await _jobService.UnsaveJobAsync(userId, jobId);
            return Ok();
        }
    }
}
