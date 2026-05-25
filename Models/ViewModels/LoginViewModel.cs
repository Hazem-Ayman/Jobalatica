using System.ComponentModel.DataAnnotations;

namespace Jobalatica.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty; // User's account email address

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty; // Private account access code

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; } // Keep user session active
    }
}
