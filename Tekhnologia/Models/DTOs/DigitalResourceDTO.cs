
namespace Tekhnologia.Models.DTOs
{
    public class DigitalResourceDTO
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty; 

        public string FileType { get; set; } = string.Empty;

        public string? Category { get; set; }

        public bool IsFree { get; set; }

        public decimal? Price { get; set; }

        public string FilePath { get; set; } = string.Empty; // URL for the file

        public DateTime UploadDate { get; set; }

        public string? ThumbnailUrl { get; set; }

        public string? ExternalUrl { get; set; }
    }
}
