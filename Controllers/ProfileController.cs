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
                JobTitle = user.JobTitle,
                CompanyName = user.CompanyName,
                ProfilePicture = user.ProfilePicture,
                AllSkills = allSkills,
                UserSkillIds = user.UserSkills.Select(us => us.SkillId).ToList(),
                SavedJobs = user.SavedJobs.ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadPhoto(IFormFile? img_file)
        {
            if (img_file == null || img_file.Length == 0) return RedirectToAction(nameof(Index));

            var userId = _userManager.GetUserId(User);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound();

            try
            {
                // Strict extension check
                var ext = Path.GetExtension(img_file.FileName).ToLower();
                var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                if (!allowed.Contains(ext)) return RedirectToAction(nameof(Index));

                // Path resolution
                var webRoot = _hostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var path = Path.Combine(webRoot, "Img");
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                // Safe filename (GUID only to avoid weird char issues)
                string fileName = $"{Guid.NewGuid()}{ext}";
                string filePath = Path.Combine(path, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await img_file.CopyToAsync(stream);
                }

                user.ProfilePicture = fileName;
                TempData["Success"] = "Thank you! Your contribution helps the community.";
                return RedirectToAction("Index", "Profile");
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText("crash.log", $"\n[{DateTime.Now}] UPLOAD FAIL: {ex.Message}\n");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveProfile(ProfileViewModel model)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound();

            try
            {
                user.DisplayName = model.DisplayName;
                user.JobTitle = model.JobTitle ?? user.JobTitle;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    var currentSkills = await _context.UserSkills.Where(us => us.UserId == user.Id).ToListAsync();
                    _context.UserSkills.RemoveRange(currentSkills);

                    if (model.UserSkillIds != null)
                    {
                        foreach (var skillId in model.UserSkillIds)
                        {
                            _context.UserSkills.Add(new UserSkill { UserId = user.Id, SkillId = skillId });
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                return await RepopulateAndReturn(model, user);
            }
        }

        private async Task<IActionResult> RepopulateAndReturn(ProfileViewModel model, ApplicationUser user)
        {
            model.AllSkills = await _context.Skills.OrderBy(s => s.Name).ToListAsync();
            model.UserSkillIds = await _context.UserSkills.Where(us => us.UserId == user.Id).Select(us => us.SkillId).ToListAsync();
            model.SavedJobs = await _context.SavedJobs.Where(sj => sj.UserId == user.Id).Include(sj => sj.Job).ToListAsync();
            model.ProfilePicture = user.ProfilePicture;
            return View("Index", model);
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
