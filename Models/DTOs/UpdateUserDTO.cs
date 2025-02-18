using System.ComponentModel.DataAnnotations;

namespace Models.DTOs
{
    public class UpdateUserDTO
    {
        public string? Name { get; set; }

        [EmailAddress]
        public string? Email { get; set; }
    }
}