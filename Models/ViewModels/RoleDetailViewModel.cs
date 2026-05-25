using Jobalatica.Models.Entities;

namespace Jobalatica.Models.ViewModels
{
    public class RoleDetailViewModel
    {
        public string JobTitle { get; set; } = string.Empty; // Title of the role analyzed
        public decimal AvgSalaryMin { get; set; } // Lowest average market pay
        public decimal AvgSalaryMax { get; set; } // Highest average market pay
        public int SampleSize { get; set; } // Number of data points used
        public List<DemandSnapshot> Trend { get; set; } = new(); // Historical hiring volume data
        public List<Job> SampleJobs { get; set; } = new(); // Live examples of this role
        public List<Skill> CommonSkills { get; set; } = new(); // Required technical skill set
        public bool HasContributedSalary { get; set; } // User's data sharing status
        public List<long> SavedJobIds { get; set; } = new(); // User's bookmarked job IDs

        public List<(string Level, decimal Min, decimal Max)> SalaryByExperience { get; set; } = new(); // Pay scale by seniority
        public List<(string Name, int Count)> TopCompanies { get; set; } = new(); // Major employers for role
        public int DemandScore { get; set; } // Role's market popularity rating
        public List<(string SkillName, int Percentage)> SkillDistribution { get; set; } = new(); // Common tech stack breakdown
        public string? CurrentLevel { get; set; } // Identified seniority level
        public RoleHealth Health { get; set; } = new(); // Overall market stability metrics
    }

    public class RoleHealth
    {
        public int Score { get; set; } // Overall market stability score
        public string Status { get; set; } = "Stable"; // General role health description
        public string StatusColor { get; set; } = "var(--green)"; // UI indicator for health
        public int DiversityScore { get; set; } // Employer variety rating score
        public int PipelineScore { get; set; } // Junior entry difficulty score
        public int ValueScore { get; set; } // Salary premium relative score
        public string Recommendation { get; set; } = string.Empty; // Career advice for role
    }
}
