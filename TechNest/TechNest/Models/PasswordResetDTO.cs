using System.ComponentModel.DataAnnotations;

namespace TechNest.Models
{
    public class PasswordResetDTO
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";
        [Required, MaxLength(100)]
        public string Password { get; set; } = "";
        [Required(ErrorMessage = "The Confirm Password field is required.")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = "";
    }
}
