using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.DTOs;

namespace Services
{
    /// <summary>
    /// Provides admin-related business logic such as retrieving users, updating profiles,
    /// promoting users, and deleting users.
    /// </summary>
    public class AdminService
    {
        private readonly UserManager<User> _userManager;

        public AdminService(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        /// <summary>
        /// Retrieves all users along with their primary role and sorts them (Admins first, then by name).
        /// </summary>
        public async Task<List<object>> GetAllUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var userList = new List<object>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault() ?? "User"; // Default role if none assigned

                userList.Add(new
                {
                    user.Id,
                    user.Name,
                    user.Email,
                    Role = role,
                    user.CreatedAt,
                    user.UpdatedAt
                });
            }

            // Sort by role (Admins first) and then by name
            var sortedUsers = userList
                .OrderByDescending(u => u.GetType().GetProperty("Role")?.GetValue(u)?.ToString() == "Admin")
                .ThenBy(u => u.GetType().GetProperty("Name")?.GetValue(u)?.ToString())
                .ToList();

            return sortedUsers;
        }

        /// <summary>
        /// Retrieves a user by their ID.
        /// </summary>
        public async Task<User?> GetUserByIdAsync(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }

        /// <summary>
        /// Updates a user's profile based on the provided DTO.
        /// </summary>
        public async Task<(bool Success, IEnumerable<string> Errors, User? UpdatedUser)> UpdateUserAsync(string id, UpdateUserDTO model)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return (false, new[] { "User not found" }, null);

            user.Name = model.Name ?? user.Name;
            user.Email = model.Email ?? user.Email;
            if (!string.IsNullOrEmpty(model.Email))
                user.UserName = model.Email; // Keep UserName in sync with Email

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
                return (true, Array.Empty<string>(), user);

            return (false, result.Errors.Select(e => e.Description), null);
        }

        /// <summary>
        /// Promotes a user to the Admin role.
        /// </summary>
        public async Task<(bool Success, IEnumerable<string> Errors)> PromoteToAdminAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return (false, new[] { "User not found" });

            if (await _userManager.IsInRoleAsync(user, "Admin"))
                return (false, new[] { "User is already an Admin" });

            var result = await _userManager.AddToRoleAsync(user, "Admin");
            if (result.Succeeded)
                return (true, Array.Empty<string>());

            return (false, result.Errors.Select(e => e.Description));
        }

        /// <summary>
        /// Deletes a user by ID, preventing deletion of protected users.
        /// </summary>
        public async Task<(bool Success, IEnumerable<string> Errors)> DeleteUserAsync(string id)
        {
            // List of protected user IDs that cannot be deleted
            var protectedUserIds = new List<string>
            {
                "682dd3de-5e64-4ba5-ba42-1cecace32402",
                "5c87f46c-2d3b-48d7-89d6-d8fcfeda1a4b"
            };

            if (protectedUserIds.Contains(id))
                return (false, new[] { "This user cannot be deleted." });

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return (false, new[] { "User not found" });

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
                return (true, Array.Empty<string>());

            return (false, result.Errors.Select(e => e.Description));
        }
    }
}
