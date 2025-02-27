using Microsoft.AspNetCore.Identity;
using Tekhnologia.Models;
using Tekhnologia.Services.Interfaces;

namespace Tekhnologia.Tests.Fakes
{
    public class FakeAuthService : IAuthService
    {
        public Task<IdentityResult> RegisterAsync(RegisterModel model)
        {
            // Always succeed in registration.
            return Task.FromResult(IdentityResult.Success);
        }

        public Task<string?> LoginAsync(LoginModel model)
        {
            // Return a dummy token if the credentials match; otherwise, return null.
            if (model.Email == "test@example.com" && model.Password == "Test@123")
            {
                return Task.FromResult<string?>("dummy_jwt_token");
            }
            return Task.FromResult<string?>(null);
        }
    }
}
