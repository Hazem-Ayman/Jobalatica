using Jobalatica.Models.Entities;

namespace Jobalatica.Models.ViewModels
{
    public class JobSearchViewModel
    {
        public List<Job> Jobs { get; set; } = new(); // Resulting job postings list
        public int TotalCount { get; set; } // Total records found
        public int CurrentPage { get; set; } = 1; // Current results page number
        public int PageSize { get; set; } = 20; // Items per search page
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize); // Total number of pages

        public string? Query { get; set; } // User's search text input
        public string? Location { get; set; } // Filtered city or region
        public string? ExperienceLevel { get; set; } // Seniority filter for jobs
        public decimal? SalaryMin { get; set; } // Minimum pay filter value
        public decimal? SalaryMax { get; set; } // Maximum pay filter value
        public List<long> SavedJobIds { get; set; } = new(); // Bookmarked job identifier list
        public List<Job> RecommendedJobs { get; set; } = new(); // Suggested roles for user
    }
}
