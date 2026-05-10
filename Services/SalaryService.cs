using Jobalatica.Data;
using Jobalatica.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Jobalatica.Services
{
    public class SalaryService : ISalaryService
    {
        private readonly ApplicationDbContext _db;

        public SalaryService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task SubmitReportAsync(SalaryReport report)
        {
            report.SubmittedAt = DateTime.UtcNow;
            _db.SalaryReports.Add(report);
            await _db.SaveChangesAsync();
        }

        public Task<List<SalaryReport>> GetReportsForRoleAsync(string jobTitle)
        {
            return _db.SalaryReports
                .Where(report => report.JobTitle.ToLower() == jobTitle.ToLower())
                .OrderByDescending(report => report.SubmittedAt)
                .ToListAsync();
        }
    }
}
