using Jobalatica.Models.Entities;

namespace Jobalatica.Models.ViewModels
{
    public class ProfileViewModel
    {
        public string DisplayName { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public List<Skill> AllSkills { get; set; } = new();
        public List<int> UserSkillIds { get; set; } = new();
        public List<SavedJob> SavedJobs { get; set; } = new();
    }
}
