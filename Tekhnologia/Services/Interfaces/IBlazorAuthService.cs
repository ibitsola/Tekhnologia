using Microsoft.AspNetCore.Identity;
using Tekhnologia.Models;

namespace Tekhnologia.Services.Interfaces
{
    public interface IBlazorAuthService
    {
        Task<IdentityResult> RegisterAsync(RegisterModel model);
        Task<string?> LoginAsync(LoginModel model);
    }
}