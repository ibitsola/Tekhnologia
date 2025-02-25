using System.ComponentModel.DataAnnotations;


namespace Models
{
    public class DigitalResource
    {
        [Key]
        public int Id { get; set; }

        [Required]
         public string Title { get; set; } = string.Empty;  // Resource title

        [Required]
        public string FileName { get; set; } = string.Empty;  // The actual file name

        [Required]
        public string FilePath { get; set; } = string.Empty;   // Path where the file is stored

        [Required]
        public string FileType { get; set; } = string.Empty;  // e.g., "pdf", "excel", "video"

        public string? Category { get; set; }  // Optional category (e.g., "Career Guide", "Coaching Kit")

        public DateTime UploadDate { get; set; } = DateTime.UtcNow;  // Automatically set date

        public bool IsFree { get; set; } = true;  // True = free, False = requires purchase

        public decimal? Price { get; set; }  // Nullable price field if not free

        public string UploadedBy { get; set; } = string.Empty;  // Name or ID of the uploader
    }
}
