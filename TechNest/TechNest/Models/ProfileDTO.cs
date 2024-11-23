using System.ComponentModel.DataAnnotations;

namespace TechNest.Models
{
    public class ProfileDTO
    {
        [Required(ErrorMessage = "The First Name field is required"), MaxLength(100)]
        public string FirstName { get; set; } = "";
        [Required(ErrorMessage = "The Last Name field is required"), MaxLength(100)]
        public string LastName { get; set; } = "";
        [Required(ErrorMessage = "The Email field is required"), EmailAddress, MaxLength(100)]
        public string Email { get; set; } = "";
        [Phone(ErrorMessage = "The format of the phone number is invalid"), MaxLength(20)]
        public string? PhoneNumber { get; set; }
        [Required, MaxLength(200)]
        public string Address { get; set; } = "";
    }
}
