using System.ComponentModel.DataAnnotations;

namespace C5.Models.ViewModels
{
    public class CreateUserViewModel
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public string? Address { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu không khớp")]
        public string ConfirmPassword { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }
    }
}
