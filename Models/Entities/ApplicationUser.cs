using Microsoft.AspNetCore.Identity;

namespace Jobalatica.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string DisplayName { get; set; } = string.Empty;
        public string ExperienceLevel { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }

        public ICollection<UserSkill> UserSkills { get; set; } = new List<UserSkill>();
        public ICollection<SalaryReport> SalaryReports { get; set; } = new List<SalaryReport>();
        public ICollection<SavedJob> SavedJobs { get; set; } = new List<SavedJob>();
    }
}
