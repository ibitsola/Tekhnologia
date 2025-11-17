using System;
using System.Threading.Tasks;

namespace Tekhnologia.UI.Services.Interfaces
{
    public interface IAuthStateService
    {
        bool IsLoggedIn { get; }
        bool IsAdmin { get; }
        bool Initialized { get; }
        event Action? OnChange;
        Task RefreshAsync();
        Task<bool> LoginAsync(string email, string password);
        Task LogoutAsync();
    }
}