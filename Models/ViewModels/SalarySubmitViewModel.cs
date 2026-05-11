using System.ComponentModel.DataAnnotations;

namespace Jobalatica.Models.ViewModels
{
    public class SalarySubmitViewModel
    {
        [Required]
        public string JobTitle { get; set; } = string.Empty;

        [Required]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        public string Location { get; set; } = string.Empty;

        [Range(0, 999999)] // Increased range slightly to be safe, spec said 99999
        public decimal Salary { get; set; }

        [Required]
        public string Currency { get; set; } = "USD";

        [Range(0, 40)]
        public int YearsExperience { get; set; }

        public string? SkillsList { get; set; }
    }
}
