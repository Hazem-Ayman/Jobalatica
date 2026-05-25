using Jobalatica.Models.Entities;

namespace Jobalatica.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; } // Count of registered users
        public int TotalJobs { get; set; } // Count of job postings
        public int TotalSalaryReports { get; set; } // Count of salary contributions
        public int TotalSavedJobs { get; set; } // Count of bookmarked jobs
        public int NewUsersToday { get; set; } // Users joined today count
        public int NewJobsToday { get; set; } // Jobs posted today count

        public List<AdminUserRow> RecentUsers { get; set; } = new(); // List of latest registrations
        public List<SalaryReport> RecentSalaryReports { get; set; } = new(); // List of latest salaries
        public List<AdminJobRow> RecentJobs { get; set; } = new(); // List of latest jobs
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
