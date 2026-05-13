using Jobalatica.Models.Entities;
using Jobalatica.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

using Jobalatica.Data;
using Microsoft.EntityFrameworkCore;

namespace Jobalatica.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IEmailSender emailSender, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
            var model = new RegisterViewModel
            {
                AllSkills = await _context.Skills.OrderBy(s => s.Name).ToListAsync()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    DisplayName = model.DisplayName,
                    JobTitle = model.JobTitle,
                    ExperienceLevel = "Unspecified",
                    CreatedAt = DateTime.UtcNow,
                    TechInterests = model.TechInterests != null ? string.Join(",", model.TechInterests) : string.Empty,
                    ProjectInterests = string.Empty
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Generate confirmation token
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

                    // Build confirmation link
                    var confirmLink = Url.Action(
                        "ConfirmEmail",
                        "Account",
                        new { userId = user.Id, token = encodedToken },
                        Request.Scheme
                    );

                    // Send email
                    try {
                        await _emailSender.SendEmailAsync(
                            user.Email!,
                            "Confirm your JobPulse account",
                            $@"
                            <div style='font-family:sans-serif;max-width:480px;margin:auto;padding:32px;'>
                                <h2 style='color:#F2708A;'>Welcome to JobPulse 👋</h2>
                                <p>Thanks for signing up. Click the button below to confirm your email address.</p>
                                <a href='{confirmLink}'
                                   style='display:inline-block;margin-top:16px;padding:12px 28px;
                                          background:#F2708A;color:#fff;border-radius:0;
                                          text-decoration:none;font-weight:600;border:2px solid #1A1612;'>
                                    Confirm My Account
                                </a>
                                <p style='margin-top:24px;color:#888;font-size:13px;'>
                                    If you didn't sign up for JobPulse, you can safely ignore this email.
                                </p>
                            </div>
                            "
                        );
                    } catch { /* Fail silently for demo */ }

                    return RedirectToAction("RegisterSuccess");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            model.AllSkills = await _context.Skills.OrderBy(s => s.Name).ToListAsync();
            return View(model);
        }

        [HttpGet]
        public IActionResult RegisterSuccess()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
                return RedirectToAction("Index", "Home");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (result.Succeeded)
            {
                TempData["Success"] = "Email confirmed! You can now log in.";
                return RedirectToAction("Login");
            }

            TempData["Error"] = "Email confirmation failed. The link may have expired.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
            return View(new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    if (string.Equals(model.Email, "hazemayman494489@gmail.com", StringComparison.OrdinalIgnoreCase))
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> MarkSalaryPromptAsSeen()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    user.HasSeenSalaryPrompt = true;
                    await _userManager.UpdateAsync(user);
                }
            }
            return Ok();
        }
    }
}
