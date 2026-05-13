using Jobalatica.Models.Entities;

namespace Jobalatica.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalJobs { get; set; }
        public int TotalSalaryReports { get; set; }
        public int TotalSavedJobs { get; set; }
        public int NewUsersToday { get; set; }
        public int NewJobsToday { get; set; }

        public List<AdminUserRow> RecentUsers { get; set; } = new();
        public List<SalaryReport> RecentSalaryReports { get; set; } = new();
        public List<AdminJobRow> RecentJobs { get; set; } = new();
    }

    public class AdminUserRow
    {
        public string Id { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class AdminJobRow
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime PostedAt { get; set; }
    }
}
