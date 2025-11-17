using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Tekhnologia.UI.Services.Interfaces;

namespace Tekhnologia.UI.Services
{
    internal sealed class AuthStateService : IAuthStateService
    {
        private readonly HttpClient _http;
        private readonly NavigationManager _nav;
        public bool IsLoggedIn { get; private set; }
        public bool IsAdmin { get; private set; }
        public bool Initialized { get; private set; }
        public event Action? OnChange;

        public AuthStateService(HttpClient http, NavigationManager nav)
        {
            _http = http;
            _nav = nav;
        }

        public async Task RefreshAsync()
        {
            try
            {
                var resp = await _http.GetAsync("/api/user/current");
                if (!resp.IsSuccessStatusCode)
                {
                    IsLoggedIn = false;
                    IsAdmin = false;
                }
                else
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;
                    IsLoggedIn = true;
                    IsAdmin = root.TryGetProperty("role", out var roleEl) && roleEl.GetString() == "Admin";
                }
            }
            catch
            {
                IsLoggedIn = false;
                IsAdmin = false;
            }
            finally
            {
                Initialized = true;
                OnChange?.Invoke();
            }
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
            var payload = new { email, password };
            var resp = await _http.PostAsJsonAsync("/api/auth/login", payload);
            if (resp.IsSuccessStatusCode)
            {
                await RefreshAsync();
                return true;
            }
            return false;
        }

        public async Task LogoutAsync()
        {
            try
            {
                await _http.PostAsync("/api/auth/logout", null);
            }
            catch { }
            IsLoggedIn = false;
            IsAdmin = false;
            OnChange?.Invoke();
        }
    }
}