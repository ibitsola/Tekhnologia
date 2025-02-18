using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class Goal
    {
        [Key]
        public Guid GoalId { get; set; } = Guid.NewGuid();

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public User? User { get; set; } // Links goal to a user

        [Required]
        public string Title { get; set; } = string.Empty; // Goal title

        public string Description { get; set; } = string.Empty; // Goal details

        public DateTime? Deadline { get; set; } // Optional completion date

        public bool IsCompleted { get; set; } = false; // Marks if goal is done

        [Required]
        public string Urgency { get; set; } = "Not Urgent"; // Eisenhower Matrix

        [Required]
        public string Importance { get; set; } = "Not Important"; // Eisenhower Matrix

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}