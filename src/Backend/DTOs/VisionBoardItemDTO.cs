using System;

namespace Backend.DTOs
{
    public class VisionBoardItemDTO
    {
        public Guid VisionId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string Caption { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
