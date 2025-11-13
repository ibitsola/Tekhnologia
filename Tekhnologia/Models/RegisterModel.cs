using System.ComponentModel.DataAnnotations;

namespace Tekhnologia.Models
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "Please provide an email address.")]
        [EmailAddress(ErrorMessage = "Please provide a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]        
        [RegularExpression("^(?=.*[A-Za-z])(?=.*\\d)(?=.*[^A-Za-z0-9]).{8,}$",
            ErrorMessage = "Password must be at least 8 characters and include at least one letter, one number, and one symbol.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please provide your name.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password.")]
        [Compare("Password", ErrorMessage = "Your passwords doesn't match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
