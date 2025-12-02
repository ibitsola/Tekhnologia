using Microsoft.AspNetCore.Components.Authorization;
using Tekhnologia.Services.Interfaces;

namespace Tekhnologia.Services
{
    public class AuthStateService : IAuthStateService
    {
        private readonly AuthenticationStateProvider _provider;
        private bool _isChecking = false;

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
            if (_isChecking)
                return;

            _isChecking = true;
            try
            {
                var state = await _provider.GetAuthenticationStateAsync();
                // debug logging to help trace auth state in UI process
                try
                {
                    var name = state?.User?.Identity?.Name ?? "(no name)";
                    var isAuth = state?.User?.Identity?.IsAuthenticated ?? false;
                    // Debug log removed for cleaner output
                }
                catch { }
                var wasLoggedIn = IsLoggedIn;
                var wasAdmin = IsAdmin;

                IsLoggedIn = state?.User?.Identity?.IsAuthenticated ?? false;
                IsAdmin = (state?.User != null) ? state.User.IsInRole("Admin") : false;

                // Only raise change if state actually changed to avoid recursive notification loops
                if (wasLoggedIn != IsLoggedIn || wasAdmin != IsAdmin)
                {
                    // Invoke subscribers asynchronously to avoid synchronous re-entrancy
                    var handler = OnChange;
                    if (handler != null)
                        _ = Task.Run(() => handler.Invoke());
                }
            }
            finally
            {
                _isChecking = false;
            }
        }

        private async void OnAuthStateChanged(Task<AuthenticationState> task)
        {
            if (_isChecking)
                return;

            _isChecking = true;
            try
            {
                var state = await task;
                var wasLoggedIn = IsLoggedIn;
                var wasAdmin = IsAdmin;

                IsLoggedIn = state?.User?.Identity?.IsAuthenticated ?? false;
                IsAdmin = (state?.User != null) ? state.User.IsInRole("Admin") : false;

                if (wasLoggedIn != IsLoggedIn || wasAdmin != IsAdmin)
                {
                    var handler = OnChange;
                    if (handler != null)
                        _ = Task.Run(() => handler.Invoke());
                }
            }
            finally
            {
                _isChecking = false;
            }
        }
    }
}
