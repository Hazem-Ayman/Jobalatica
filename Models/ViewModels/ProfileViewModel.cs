using Jobalatica.Models.Entities;
using Microsoft.AspNetCore.Http;

namespace Jobalatica.Models.ViewModels
{
    public class ProfileViewModel
    {
        public string DisplayName { get; set; } = string.Empty; // User's public display name
        public string JobTitle { get; set; } = string.Empty; // User's current professional title
        public string CompanyName { get; set; } = string.Empty; // User's current employer name
        public string? ProfilePictureUrl { get; set; } // Web link to avatar
        public IFormFile? ProfilePicture { get; set; } // Uploaded image file data
        public List<Skill> AllSkills { get; set; } = new(); // Available skills for selection
        public List<int> UserSkillIds { get; set; } = new(); // Selected user skill identifiers
        public List<SavedJob> SavedJobs { get; set; } = new(); // List of bookmarked roles
    }
}
