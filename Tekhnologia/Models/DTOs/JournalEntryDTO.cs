namespace Tekhnologia.Models.DTOs
{
    public class JournalEntryDTO
    {
        public Guid EntryId { get; set; } 
        public string EntryText { get; set; } = string.Empty;
        public int? SentimentScore { get; set; }
        public bool Visibility { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}