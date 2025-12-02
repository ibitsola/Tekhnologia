namespace Tekhnologia.Models.DTOs
{
   public class CreateVisionBoardItemDTO
    {
        public string ImageUrl { get; set; } = string.Empty;
        public string Caption { get; set; } = string.Empty;
        public int PositionX { get; set; } = 0;
        public int PositionY { get; set; } = 0;
        public int? Width { get; set; }
        public int? Height { get; set; }
    }
}