using Jobalatica.Models.Entities;

namespace Jobalatica.Models.ViewModels
{
    public class HomeViewModel
    {
        public List<(string Title, int Count)> TopRoles { get; set; } = new(); // Trending job titles list
        public List<Skill> TopSkills { get; set; } = new(); // Most popular tech skills
        public List<Job> RecentJobs { get; set; } = new(); // Latest job postings feed
        public List<Job>? PersonalizedJobs { get; set; } // Recommended jobs for user
        public List<long> SavedJobIds { get; set; } = new(); // IDs of bookmarked jobs
    }
}
