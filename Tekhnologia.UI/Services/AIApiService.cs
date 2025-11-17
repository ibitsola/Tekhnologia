using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Tekhnologia.UI.Services.Interfaces;

namespace Tekhnologia.UI.Services
{
    internal sealed class AIApiService : IAIApiService
    {
        private readonly HttpClient _http;
        public AIApiService(HttpClient http) => _http = http;

        public async Task<string?> GetBusinessCoachingResponseAsync(string message)
        {
            var payload = new ChatRequestDTO { Message = message };
            var resp = await _http.PostAsJsonAsync("/api/chatbot/business-coach", payload);
            if (!resp.IsSuccessStatusCode) return null;
            var stream = await resp.Content.ReadAsStreamAsync();
            var dto = await JsonSerializer.DeserializeAsync<ChatbotResponseDTO>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return dto?.Response;
        }
    }
}