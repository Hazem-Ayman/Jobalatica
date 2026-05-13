using Jobalatica.Models.Entities;

namespace Jobalatica.Models.ViewModels
{
    public class HomeViewModel
    {
        public List<(string Title, int Count)> TopRoles { get; set; } = new();
        public List<Skill> TopSkills { get; set; } = new();
        public List<Job> RecentJobs { get; set; } = new();
        public List<Job>? PersonalizedJobs { get; set; }
        public List<long> SavedJobIds { get; set; } = new();
    }
}
