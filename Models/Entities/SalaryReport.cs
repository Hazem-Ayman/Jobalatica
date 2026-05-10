namespace Jobalatica.Models.Entities
{
    public class SalaryReport
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public decimal Salary { get; set; }
        public string Currency { get; set; } = "USD";
        public int YearsExperience { get; set; }
        public string SkillsList { get; set; } = string.Empty;
        public bool IsVerified { get; set; } = false;
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public ApplicationUser? User { get; set; }
    }
}
