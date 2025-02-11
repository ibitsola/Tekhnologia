using System;
using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class VisionBoardItem
    {
        [Key]
        public Guid VisionId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; } // Foreign Key to Users

        [Required]
        public string ImageUrl { get; set; } = string.Empty; // Path to image storage

        public string Caption { get; set; } = string.Empty; // Optional text description

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
