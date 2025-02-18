using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class JournalEntry
    {
        [Key] 
        public Guid EntryId { get; set; } = Guid.NewGuid();

        public string UserId { get; set; } = string.Empty; 

        [ForeignKey("UserId")]
        public User? User { get; set; } // Defines the relationship

        [Required]
        public DateTime Date { get; set; } = DateTime.UtcNow;

        [Required]
        public string EntryText { get; set; } = string.Empty;

        public int? SentimentScore { get; set; }

        public bool Visibility { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}