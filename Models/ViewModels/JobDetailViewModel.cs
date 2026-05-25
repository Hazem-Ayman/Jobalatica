using Jobalatica.Models.Entities;

namespace Jobalatica.Models.ViewModels
{
    public class JobDetailViewModel
    {
        public Job Job { get; set; } = null!; // Full job post details
        public bool IsSaved { get; set; } // Whether user bookmarked this
        public List<Job> SimilarJobs { get; set; } = new(); // Other related job postings

        // Skill Gap Analysis
        public List<Skill> MatchingSkills { get; set; } = new(); // Skills the user has
        public List<Skill> MissingSkills { get; set; } = new(); // Skills the user lacks
        public int MatchPercentage { get; set; } // Overall profile match score
        public int DemandScore { get; set; } // Real-time market popularity
    }
}
