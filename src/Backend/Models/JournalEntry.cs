using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class JournalEntry
    {
        [Key] // Marks this as the Primary Key
        public Guid EntryId { get; set; } = Guid.NewGuid(); // Unique ID for each journal entry (UUID)

        [Required]
        public Guid UserId { get; set; } // Links this entry to a specific user (Foreign Key)

        [Required]
        public DateTime Date { get; set; } = DateTime.UtcNow; // The date of the entry

        [Required]
        public string EntryText { get; set; } = string.Empty; // The journal content

        public int? SentimentScore { get; set; } // Optional AI-generated sentiment analysis score

        public bool Visibility { get; set; } = true; // Public (true) or Private (false)

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
