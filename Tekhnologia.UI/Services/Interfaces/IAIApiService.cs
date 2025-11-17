using System.Threading.Tasks;

namespace Tekhnologia.UI.Services.Interfaces
{
    public interface IAIApiService
    {
        Task<string?> GetBusinessCoachingResponseAsync(string message);
    }

    public sealed class ChatRequestDTO
    {
        public string? Message { get; set; }
    }

    public sealed class ChatbotResponseDTO
    {
        public string? Response { get; set; }
    }
}