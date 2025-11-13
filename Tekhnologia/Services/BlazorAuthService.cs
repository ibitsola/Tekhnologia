using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Tekhnologia.Models;
using Tekhnologia.Services.Interfaces;

namespace Tekhnologia.Services
{
    public class BlazorAuthService : IBlazorAuthService
    {
        private readonly HttpClient _http;

        public BlazorAuthService(HttpClient http)
        {
            _http = http;
        }

        public async Task<string?> LoginAsync(LoginModel model)
        {
            var response = await _http.PostAsJsonAsync("/api/auth/login", model);
            return response.IsSuccessStatusCode ? model.Email : null;
        }

        public async Task<IdentityResult> RegisterAsync(RegisterModel model)
        {
            var response = await _http.PostAsJsonAsync("/api/auth/register", model);
            if (response.IsSuccessStatusCode)
                return IdentityResult.Success;

            var json = await response.Content.ReadAsStringAsync();
            var errors = JsonSerializer.Deserialize<List<IdentityError>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new();

            return IdentityResult.Failed(errors.ToArray());
        }
    }
}
