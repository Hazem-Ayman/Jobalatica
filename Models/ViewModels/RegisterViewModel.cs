using System.ComponentModel.DataAnnotations;

namespace Jobalatica.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "Display Name")]
        public string DisplayName { get; set; } = string.Empty; // User's visible profile name

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty; // Login and contact email

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty; // Account security secret key

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty; // Verification of the password

        [Required]
        [Display(Name = "Job Title")]
        public string JobTitle { get; set; } = string.Empty; // User's current professional role

        public List<string> TechInterests { get; set; } = new(); // Skills user likes
        public List<Jobalatica.Models.Entities.Skill> AllSkills { get; set; } = new(); // Full list for selection
    }
}
