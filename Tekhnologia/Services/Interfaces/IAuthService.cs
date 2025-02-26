using Microsoft.AspNetCore.Identity;
using Tekhnologia.Models;

namespace Tekhnologia.Services.Interfaces
{
    public interface IAuthService
    {
        Task<IdentityResult> RegisterAsync(RegisterModel model);
        Task<string?> LoginAsync(LoginModel model);
    }
}
