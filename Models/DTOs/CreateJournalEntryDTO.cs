namespace Models.DTOs
{
    public class CreateJournalEntryDTO
    {
        public string EntryText { get; set; } = string.Empty;

        public int? SentimentScore { get; set; }
        
        public bool Visibility { get; set; }
    }
}