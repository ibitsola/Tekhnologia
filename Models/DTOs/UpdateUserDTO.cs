using System.ComponentModel.DataAnnotations;

namespace Tekhnologia.Models.DTOs
{
    public class UpdateUserDTO
    {
        public string? Name { get; set; }

        [EmailAddress]
        public string? Email { get; set; }
    }
}