using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace C5.Models.ViewModels
{
    public class EditUserViewModel 
    {
        
        public string Id { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        public string FullName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; }
    }
}
