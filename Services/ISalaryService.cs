using Jobalatica.Models.Entities;

namespace Jobalatica.Services
{
    public interface ISalaryService
    {
        Task SubmitReportAsync(SalaryReport report);
        Task<List<SalaryReport>> GetReportsForRoleAsync(string jobTitle);
    }
}
