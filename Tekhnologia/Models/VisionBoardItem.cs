using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tekhnologia.Models
{
    public class VisionBoardItem
    {
        [Key]
        public Guid VisionId { get; set; } = Guid.NewGuid();

        [Required]
        public string UserId { get; set; } = string.Empty; 
        [ForeignKey("UserId")]
        public User? User { get; set; } // Defines the relationship

        [Required]
        public string ImageUrl { get; set; } = string.Empty;

        public string Caption { get; set; } = string.Empty;

        public int PositionX { get; set; } = 0;
        public int PositionY { get; set; } = 0;

        // Optional persisted width/height for saved sizing
        public int? Width { get; set; }
        public int? Height { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}