using System;
using System.ComponentModel.DataAnnotations;

namespace Tekhnologia.Models
{
    public class Conversation
    {
        [Key]
        public Guid ConversationId { get; set; } = Guid.NewGuid();
        public string UserId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty; // optional title for quick reference
        public string MessagesJson { get; set; } = string.Empty; // store messages as JSON
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
