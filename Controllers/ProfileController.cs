using Jobalatica.Data;
using Jobalatica.Models.Entities;
using Jobalatica.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Jobalatica.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProfileController(
            UserManager<ApplicationUser> userManager, 
            ApplicationDbContext context,
            IWebHostEnvironment hostEnvironment)
        {
            _userManager = userManager;
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.Users
                .Include(u => u.UserSkills)
                .Include(u => u.SavedJobs)
                    .ThenInclude(sj => sj.Job)
                .FirstOrDefaultAsync(u => u.Id == _userManager.GetUserId(User));

            if (user == null) return NotFound();

            var allSkills = await _context.Skills.OrderBy(s => s.Name).ToListAsync();

            var vm = new ProfileViewModel
            {
                DisplayName = user.DisplayName,
                ExperienceLevel = user.ExperienceLevel,
                AllSkills = allSkills,
                UserSkillIds = user.UserSkills.Select(us => us.SkillId).ToList(),
                SavedJobs = user.SavedJobs.ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveProfile(ProfileViewModel model)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound();

            user.DisplayName = model.DisplayName ?? user.DisplayName;
            user.ExperienceLevel = model.ExperienceLevel ?? user.ExperienceLevel;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                // Update Skills - Efficiently
                var currentSkills = await _context.UserSkills.Where(us => us.UserId == user.Id).ToListAsync();
                _context.UserSkills.RemoveRange(currentSkills);

                if (model.UserSkillIds != null && model.UserSkillIds.Any())
                {
                    foreach (var skillId in model.UserSkillIds)
                    {
                        _context.UserSkills.Add(new UserSkill { UserId = user.Id, SkillId = skillId });
                    }
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Profile updated!";
            }
            
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> SavedJobs()
        {
            var userId = _userManager.GetUserId(User);
            var savedJobs = await _context.SavedJobs
                .Include(sj => sj.Job)
                    .ThenInclude(j => j.JobSkills)
                        .ThenInclude(js => js.Skill)
                .Where(sj => sj.UserId == userId)
                .ToListAsync();

            return View(savedJobs);
        }
    }
}
