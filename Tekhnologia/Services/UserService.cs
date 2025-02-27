using Microsoft.AspNetCore.Identity;
using Tekhnologia.Models;
using Tekhnologia.Models.DTOs;
using Tekhnologia.Services.Interfaces; 

namespace Tekhnologia.Services
{
    /// <summary>
    /// Provides user-related business logic such as fetching profiles, updating profile data, and changing passwords.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public UserService(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        /// Retrieves the profile of a user by their user ID.
        /// </summary>
        public async Task<User?> GetUserProfileAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        /// <summary>
        /// Updates the profile of a user using the provided update model.
        /// </summary>
        /// <returns>A tuple containing a success flag, a list of error messages (if any), and the updated user.</returns>
        public async Task<(bool Success, IEnumerable<string> Errors, User? UpdatedUser)> UpdateUserProfileAsync(string userId, UpdateUserDTO model)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return (false, new[] { "User not found" }, null);
            }

            // Update only provided values.
            if (!string.IsNullOrEmpty(model.Name))
            {
                user.Name = model.Name;
            }
            if (!string.IsNullOrEmpty(model.Email))
            {
                user.Email = model.Email;
                user.UserName = model.Email; // Keep UserName in sync if needed.
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return (true, Array.Empty<string>(), user);
            }
            return (false, result.Errors.Select(e => e.Description), null);
        }

        /// <summary>
        /// Changes the password for a user, signing them out on success.
        /// </summary>
        /// <returns>A tuple containing a success flag and a list of error messages (if any).</returns>
        public async Task<(bool Success, IEnumerable<string> Errors)> UpdateUserPasswordAsync(string userId, UpdatePasswordDTO model)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return (false, new[] { "User not found" });
            }

            var passwordCheck = await _userManager.CheckPasswordAsync(user, model.OldPassword);
            if (!passwordCheck)
            {
                return (false, new[] { "Incorrect old password" });
            }

            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.SignOutAsync();
                return (true, Array.Empty<string>());
            }
            return (false, result.Errors.Select(e => e.Description));
        }
    }
}
