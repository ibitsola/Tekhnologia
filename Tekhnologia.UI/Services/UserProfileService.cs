using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Tekhnologia.UI.Services.Interfaces;

namespace Tekhnologia.UI.Services
{
    internal sealed class UserProfileService : IUserProfileService
    {
        private readonly HttpClient _http;
        public UserProfileService(HttpClient http) => _http = http;

        public async Task<UserInfo?> GetProfileAsync(string userId)
        {
            var resp = await _http.GetAsync($"/api/user/{userId}");
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

        public async Task<(bool Success, IEnumerable<string> Errors)> UpdateNameAsync(string userId, string name)
        {
            var payload = new { name };
            var resp = await _http.PutAsJsonAsync($"/api/user/{userId}", payload);
            if (resp.IsSuccessStatusCode) return (true, Enumerable.Empty<string>());
            return (false, new[] { await resp.Content.ReadAsStringAsync() });
        }

        public async Task<(bool Success, IEnumerable<string> Errors)> UpdatePasswordAsync(string userId, string oldPassword, string newPassword)
        {
            var payload = new { oldPassword, newPassword };
            var resp = await _http.PutAsJsonAsync($"/api/user/{userId}/password", payload);
            if (resp.IsSuccessStatusCode) return (true, Enumerable.Empty<string>());
            return (false, new[] { await resp.Content.ReadAsStringAsync() });
        }
    }
}