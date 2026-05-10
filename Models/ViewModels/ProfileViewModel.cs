using Jobalatica.Models.Entities;

namespace Jobalatica.Models.ViewModels
{
    public class ProfileViewModel
    {
        public string DisplayName { get; set; } = string.Empty;
        public string ExperienceLevel { get; set; } = string.Empty;
        public List<Skill> AllSkills { get; set; } = new();
        public List<int> UserSkillIds { get; set; } = new();
        public List<SavedJob> SavedJobs { get; set; } = new();
    }
}
