using Microsoft.AspNetCore.Identity;

namespace Jobalatica.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string DisplayName { get; set; } = string.Empty;
        public string ExperienceLevel { get; set; } = "Unspecified";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public bool HasSeenSalaryPrompt { get; set; }
        public string TechInterests { get; set; } = string.Empty;
        public string ProjectInterests { get; set; } = string.Empty;

        public ICollection<UserSkill> UserSkills { get; set; } = new List<UserSkill>();
        public ICollection<SalaryReport> SalaryReports { get; set; } = new List<SalaryReport>();
        public ICollection<SavedJob> SavedJobs { get; set; } = new List<SavedJob>();
    }
}
