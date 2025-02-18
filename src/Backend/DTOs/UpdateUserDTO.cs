using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs
{
    public class UpdateUserDTO
    {
        public string? Name { get; set; }

        [EmailAddress]
        public string? Email { get; set; }
    }
}