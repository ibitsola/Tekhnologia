using Microsoft.AspNetCore.Identity;
using Tekhnologia.Models;
using Tekhnologia.Services.Interfaces;

namespace Tekhnologia.Services
{
    // Refactored to use Identity directly (no HttpClient round trip)
    public class BlazorAuthService : IBlazorAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public BlazorAuthService(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<string?> LoginAsync(LoginModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                return null;

            await _signInManager.SignInAsync(user, isPersistent: false);
            return user.Email;
        }

        public async Task<IdentityResult> RegisterAsync(RegisterModel model)
        {
            var user = new User
            {
                Email = model.Email,
                UserName = model.Email,
                Name = model.Name
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            return result;
        }
    }
}
