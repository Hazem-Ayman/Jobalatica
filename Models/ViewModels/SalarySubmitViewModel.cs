using System.ComponentModel.DataAnnotations;

namespace Jobalatica.Models.ViewModels
{
    public class SalarySubmitViewModel
    {
        [Required]
        public string JobTitle { get; set; } = string.Empty; // Professional title of the user

        [Required]
        public string CompanyName { get; set; } = string.Empty; // Name of the user's employer

        [Required]
        public string Location { get; set; } = string.Empty; // City or region of work

        [Range(0, 999999)] // Increased range slightly to be safe, spec said 99999
        public decimal Salary { get; set; } // Annual or monthly pay amount

        [Required]
        public string Currency { get; set; } = "USD"; // Type of money for salary

        [Range(0, 40)]
        public int YearsExperience { get; set; } // Total years in the role

        public string? SkillsList { get; set; } // Comma-separated list of tech
    }
}
