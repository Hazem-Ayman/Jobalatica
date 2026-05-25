using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jobalatica.Models.Entities
{
    public class SalaryReport
    {
        [Key]
        public int Id { get; set; }

        public string? UserId { get; set; }

        [Required]
        [StringLength(200)]
        public string JobTitle { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Location { get; set; } = string.Empty;

        public decimal Salary { get; set; }

        [StringLength(10)]
        public string Currency { get; set; } = "USD";

        public int YearsExperience { get; set; }

        public string SkillsList { get; set; } = string.Empty;

        public bool IsVerified { get; set; } = false;

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }
    }
}
