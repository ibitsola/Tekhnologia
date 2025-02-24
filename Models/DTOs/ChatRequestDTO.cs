using System.Runtime.CompilerServices;

namespace Models.DTOs
{
    public class ChatRequestDTO
    {
        public string Message { get; set; } = string.Empty;
        public string? UserId { get; set; }
    }
}
