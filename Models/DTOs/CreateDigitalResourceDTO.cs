using System.ComponentModel.DataAnnotations;

namespace Tekhnologia.Models.DTOs
{
    public class CreateDigitalResourceDTO
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public required IFormFile File { get; set; }  // Uploaded file

        public string? Category { get; set; }

        public bool IsFree { get; set; }

        public decimal? Price { get; set; }  // Nullable price field
    }
}
