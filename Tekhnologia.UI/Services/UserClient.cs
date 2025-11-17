using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Tekhnologia.UI.Services.Interfaces;

namespace Tekhnologia.UI.Services
{
    internal sealed class UserClient : IUserClient
    {
        private readonly HttpClient _http;
        public UserClient(HttpClient http) => _http = http;

        public async Task<UserInfo?> GetCurrentUserAsync()
        {
            var resp = await _http.GetAsync("/api/user/current");
            if (!resp.IsSuccessStatusCode) return null;
            var json = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            return new UserInfo
            {
                Id = root.TryGetProperty("id", out var idEl) ? idEl.GetString() : null,
                Name = root.TryGetProperty("name", out var nameEl) ? nameEl.GetString() : null,
                Email = root.TryGetProperty("email", out var emailEl) ? emailEl.GetString() : null,
                Role = root.TryGetProperty("role", out var roleEl) ? roleEl.GetString() : null
            };
        }
    }
}