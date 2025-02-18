using System.ComponentModel.DataAnnotations;

namespace Models.DTOs
{
    public class UpdatePasswordDTO
    {
        [Required]
        public string OldPassword { get; set; } = string.Empty;

        [Required]
        [MinLength(8, ErrorMessage = "New password must be at least 8 characters long.")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Password must contain at least one letter, one number, and one special character.")]
        public string NewPassword { get; set; } = string.Empty;
    }
}