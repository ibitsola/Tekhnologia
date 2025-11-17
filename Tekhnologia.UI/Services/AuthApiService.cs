using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Tekhnologia.UI.Services.Interfaces;

namespace Tekhnologia.UI.Services
{
    internal sealed class AuthApiService : IAuthApiService
    {
        private readonly HttpClient _http;
        public AuthApiService(HttpClient http) => _http = http;

        public async Task<(bool Success, IEnumerable<string> Errors)> RegisterAsync(RegisterRequest request)
        {
            var resp = await _http.PostAsJsonAsync("/api/auth/register", request);
            if (resp.IsSuccessStatusCode)
            {
                return (true, Enumerable.Empty<string>());
            }

            var errors = new List<string>();
            try
            {
                var text = await resp.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    // If JSON with errors property
                    if (text.StartsWith("{"))
                    {
                        using var doc = JsonDocument.Parse(text);
                        if (doc.RootElement.TryGetProperty("errors", out var errorsEl) && errorsEl.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var e in errorsEl.EnumerateArray())
                                errors.Add(e.GetString() ?? "Unknown error");
                        }
                        else
                        {
                            errors.Add(text);
                        }
                    }
                    else
                    {
                        errors.Add(text);
                    }
                }
            }
            catch
            {
                errors.Add("Registration failed.");
            }

            return (false, errors);
        }
    }
}