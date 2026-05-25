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
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        // Add your admin email here
        private const string AdminEmail = "hazemayman494489@gmail.com";

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private bool IsAdmin()
        {
            var email = User.Identity?.Name;
            return string.Equals(email, AdminEmail, StringComparison.OrdinalIgnoreCase);
        }

        [HttpGet]
        public async Task<IActionResult> Index() // Loads platform overview stats
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Home");

            var today = DateTime.UtcNow.Date;

            var vm = new AdminDashboardViewModel
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalJobs = await _context.Jobs.CountAsync(),
                TotalSalaryReports = await _context.SalaryReports.CountAsync(),
                TotalSavedJobs = await _context.SavedJobs.CountAsync(),
                NewUsersToday = await _context.Users.CountAsync(u => u.CreatedAt >= today),
                NewJobsToday = await _context.Jobs.CountAsync(j => j.PostedAt >= today),

                RecentUsers = await _context.Users
                    .OrderByDescending(u => u.CreatedAt)
                    .Take(10)
                    .Select(u => new AdminUserRow
                    {
                        Id = u.Id,
                        DisplayName = u.DisplayName,
                        Email = u.Email ?? "",
                        JobTitle = u.JobTitle,
                        CreatedAt = u.CreatedAt
                    })
                    .ToListAsync(),

                RecentSalaryReports = await _context.SalaryReports
                    .OrderByDescending(r => r.SubmittedAt)
                    .Take(10)
                    .ToListAsync(),

                RecentJobs = await _context.Jobs
                    .OrderByDescending(j => j.PostedAt)
                    .Take(10)
                    .Select(j => new AdminJobRow
                    {
                        Id = j.Id,
                        Title = j.Title,
                        Company = j.Company,
                        Location = j.Location,
                        PostedAt = j.PostedAt
                    })
                    .ToListAsync()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id) // Permanently removes a user
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Home");

            var user = await _userManager.FindByIdAsync(id);
            if (user != null && !string.Equals(user.Email, AdminEmail, StringComparison.OrdinalIgnoreCase))
            {
                // Delete related data first to avoid FK constraint errors in SQLite
                var savedJobs = await _context.SavedJobs.Where(sj => sj.UserId == id).ToListAsync();
                _context.SavedJobs.RemoveRange(savedJobs);

                var userSkills = await _context.UserSkills.Where(us => us.UserId == id).ToListAsync();
                _context.UserSkills.RemoveRange(userSkills);

                var salaryReports = await _context.SalaryReports.Where(sr => sr.UserId == id).ToListAsync();
                foreach (var report in salaryReports)
                {
                    report.UserId = null;
                }

                await _context.SaveChangesAsync();
                await _userManager.DeleteAsync(user);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteJob(long id) // Removes a job posting
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Home");

            var job = await _context.Jobs.FindAsync(id);
            if (job != null)
            {
                _context.Jobs.Remove(job);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSalaryReport(int id) // Removes a salary entry
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Home");

            var report = await _context.SalaryReports.FindAsync(id);
            if (report != null)
            {
                _context.SalaryReports.Remove(report);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
