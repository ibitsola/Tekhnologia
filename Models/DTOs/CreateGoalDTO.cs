using System.ComponentModel.DataAnnotations;

namespace Models.DTOs
{
    public class CreateGoalDTO
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime? Deadline { get; set; }

        public bool IsCompleted { get; set; } = false; 

        [Required]
        public string Urgency { get; set; } = "Not Urgent"; // Eisenhower Matrix

        [Required]
        public string Importance { get; set; } = "Not Important"; // Eisenhower Matrix
    }
}