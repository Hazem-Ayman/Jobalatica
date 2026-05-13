namespace Jobalatica.Models.Entities
{
    public class Job
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        public string Currency { get; set; } = "USD";
        public string ExperienceLevel { get; set; } = string.Empty;
        public string SourceUrl { get; set; } = string.Empty;
        public string SourceSite { get; set; } = string.Empty;
        public DateTime PostedAt { get; set; }
        public DateTime ScrapedAt { get; set; }
        public bool IsSalaryEstimated { get; set; } = false;
        public decimal? EstimatedSalaryMin { get; set; }
        public decimal? EstimatedSalaryMax { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<JobSkill> JobSkills { get; set; } = new List<JobSkill>();
        public ICollection<SavedJob> SavedJobs { get; set; } = new List<SavedJob>();
    }
}
