namespace Tekhnologia.Services.Interfaces
{
    public interface IAuthStateService
    {
        bool IsLoggedIn { get; }
        event Action? OnChange;
        Task CheckAuthStatus(); 
    }
}
