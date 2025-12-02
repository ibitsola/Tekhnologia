using System;

namespace Tekhnologia.Models.DTOs
{
    public class VisionBoardItemDTO
    {
        public Guid VisionId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string Caption { get; set; } = string.Empty;
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}