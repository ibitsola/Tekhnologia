using System.ComponentModel.DataAnnotations;

namespace Tekhnologia.Models.DTOs
{
    public class CreateDigitalResourceDTO
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        public IFormFile? File { get; set; }  // Uploaded file (optional for courses)

        public string? Category { get; set; }

        public bool IsFree { get; set; }

        public decimal? Price { get; set; }  // Nullable price field

        public string? ThumbnailUrl { get; set; }  // Optional thumbnail image URL

        public string? ExternalUrl { get; set; }  // For courses: external link
    }
}
