using Jobalatica.Models.Entities;
using Jobalatica.Models.ViewModels;
using Jobalatica.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Jobalatica.Controllers
{
    public class SalaryController : Controller
    {
        private readonly ISalaryService _salaryService;
        private readonly UserManager<ApplicationUser> _userManager;

        public SalaryController(ISalaryService salaryService, UserManager<ApplicationUser> userManager)
        {
            _salaryService = salaryService;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Submit()
        {
            return View(new SalarySubmitViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Submit(SalarySubmitViewModel model)
        {
            if (ModelState.IsValid)
            {
                var report = new SalaryReport
                {
                    JobTitle = model.JobTitle,
                    Location = model.Location,
                    Salary = model.Salary,
                    Currency = model.Currency,
                    YearsExperience = model.YearsExperience,
                    SkillsList = model.SkillsList,
                    SubmittedAt = DateTime.UtcNow,
                    UserId = _userManager.GetUserId(User) ?? null
                };

                await _salaryService.SubmitReportAsync(report);

                TempData["Success"] = "Thank you! Your contribution helps the community.";
                return RedirectToAction(nameof(Submit));
            }

            return View(model);
        }
    }
}
