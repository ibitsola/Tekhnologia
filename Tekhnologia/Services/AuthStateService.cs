using Microsoft.AspNetCore.Components.Authorization;
using Tekhnologia.Services.Interfaces;

namespace Tekhnologia.Services
{
    public class AuthStateService : IAuthStateService
    {
        private readonly AuthenticationStateProvider _provider;

        public bool IsLoggedIn { get; private set; }
        public bool IsAdmin { get; private set; }

        public event Action? OnChange;

        public AuthStateService(AuthenticationStateProvider provider)
        {
            _provider = provider;
            _provider.AuthenticationStateChanged += OnAuthStateChanged;
            _ = CheckAuthStatus(); // prime the state
        }

        public async Task CheckAuthStatus()
        {
            var state = await _provider.GetAuthenticationStateAsync();
            IsLoggedIn = state.User.Identity?.IsAuthenticated ?? false;
            IsAdmin = state.User.IsInRole("Admin");
            OnChange?.Invoke();
        }

        private async void OnAuthStateChanged(Task<AuthenticationState> task)
        {
            var state = await task;
            IsLoggedIn = state.User.Identity?.IsAuthenticated ?? false;
            IsAdmin = state.User.IsInRole("Admin");
            OnChange?.Invoke();
        }
    }
}
