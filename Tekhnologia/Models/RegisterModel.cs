using System.ComponentModel.DataAnnotations;

namespace Tekhnologia.Models
{
    public class RegisterModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Name { get; set;} = string.Empty;
    }
}