using System;

namespace Models.DTOs
{
    public class VisionBoardItemDTO
    {
        public Guid VisionId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string Caption { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}