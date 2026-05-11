using Jobalatica.Models.Entities;
using Jobalatica.Models.ViewModels;
using Jobalatica.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public async Task<IActionResult> Submit()
        {
            var model = new SalarySubmitViewModel();
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = _userManager.GetUserId(User);
                var user = await _userManager.FindByIdAsync(userId!);
                if (user != null)
                {
                    model.JobTitle = user.JobTitle;
                    model.CompanyName = user.CompanyName;
                    
                    // Fetch skills from the UserSkill matrix
                    var userSkills = await _userManager.Users
                        .Where(u => u.Id == userId)
                        .SelectMany(u => u.UserSkills.Select(us => us.Skill.Name))
                        .ToListAsync();

                    if (userSkills.Any())
                    {
                        model.SkillsList = string.Join(", ", userSkills);
                    }
                    else
                    {
                        model.SkillsList = user.TechInterests;
                    }
                }
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Submit(SalarySubmitViewModel model)
        {
            if (ModelState.IsValid)
            {
                var report = new SalaryReport
                {
                    JobTitle = model.JobTitle,
                    CompanyName = model.CompanyName,
                    Location = model.Location,
                    Salary = model.Salary,
                    Currency = model.Currency,
                    YearsExperience = model.YearsExperience,
                    SkillsList = model.SkillsList,
                    SubmittedAt = DateTime.UtcNow,
                    UserId = _userManager.GetUserId(User) ?? null
                };

                await _salaryService.SubmitReportAsync(report);

                // Sync profile with latest job info
                if (User.Identity?.IsAuthenticated == true)
                {
                    var userId = _userManager.GetUserId(User);
                    var user = await _userManager.FindByIdAsync(userId!);
                    if (user != null)
                    {
                        user.JobTitle = model.JobTitle;
                        user.CompanyName = model.CompanyName;
                        await _userManager.UpdateAsync(user);
                    }
                }

                TempData["Success"] = "Thank you! Your contribution helps the community.";
                return RedirectToAction("Index", "Profile");
            }

            return View(model);
        }
    }
}
