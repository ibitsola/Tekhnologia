using System;
using System.Collections.Generic;

namespace Tekhnologia.Models.DTOs
{
    public class ConversationSummaryDTO
    {
        public Guid ConversationId { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
    }

    public class ConversationCreateDTO
    {
        public string Title { get; set; } = string.Empty;
        public string MessagesJson { get; set; } = string.Empty;
    }

    public class ConversationDetailDTO
    {
        public Guid ConversationId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string MessagesJson { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
