namespace Models.DTOs
{
    public class GoalResponseDTO
    {
        public Guid GoalId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime? Deadline { get; set; }

        public bool IsCompleted { get; set; }

        public string Urgency { get; set; } = "Not Urgent"; 

        public string Importance { get; set; } = "Not Important"; 

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}