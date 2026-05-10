using Jobalatica.Models.Entities;

namespace Jobalatica.Models.ViewModels
{
    public class RoleDetailViewModel
    {
        public string JobTitle { get; set; } = string.Empty;
        public decimal AvgSalaryMin { get; set; }
        public decimal AvgSalaryMax { get; set; }
        public int SampleSize { get; set; }
        public List<DemandSnapshot> Trend { get; set; } = new();
        public List<Job> SampleJobs { get; set; } = new();
        public List<Skill> CommonSkills { get; set; } = new();
        public bool HasContributedSalary { get; set; }
    }
}
