using Jobalatica.Models.Entities;

namespace Jobalatica.Models.ViewModels
{
    public class JobSearchViewModel
    {
        public List<Job> Jobs { get; set; } = new();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        public string? Query { get; set; }
        public string? Location { get; set; }
        public string? ExperienceLevel { get; set; }
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
    }
}
