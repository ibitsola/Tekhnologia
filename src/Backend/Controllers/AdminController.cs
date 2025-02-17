using Backend.DTOs;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")] // Restrict all routes to Admins
    public class AdminController : ControllerBase
    {
        private readonly UserManager<User> _userManager;

        public AdminController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        // Get all users (admin only) sorted by role and name
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var userList = new List<object>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault() ?? "User"; // Default to "User" if no role assigned

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

            // Sort by role first (Admins first, then Users) and then by name
            var sortedUsers = userList
                .OrderByDescending(u => u.GetType().GetProperty("Role")?.GetValue(u)?.ToString() == "Admin")
                .ThenBy(u => u.GetType().GetProperty("Name")?.GetValue(u)?.ToString())
                .ToList();

            return Ok(sortedUsers);
        }



        // Get a user by ID (Admins only)
        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound("User not found");

            return Ok(new { user.Id, user.Name, user.Email, user.CreatedAt, user.UpdatedAt });
        }

        // Update any user (Admins only)
        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDTO model)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound("User not found");

            user.Name = model.Name ?? user.Name;
            user.Email = model.Email ?? user.Email;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok(new { message = $"User {user.Name} updated successfully", user });
        }

        // Promote a user to Admin (Admins only)
        [HttpPost("promote/{id}")]
        public async Task<IActionResult> PromoteToAdmin(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound("User not found");

            if (await _userManager.IsInRoleAsync(user, "Admin"))
                return BadRequest("User is already an Admin");

            await _userManager.AddToRoleAsync(user, "Admin");

            return Ok(new { message = $"{user.Name} has been promoted to Admin!" });
        }

        // Delete a user (Admins only)
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            // List of users that cannot be deleted
            var protectedUserIds = new List<string>
            {
               "b74664d9-398e-40f9-a4ec-b4520318f762", 
               "6b051706-09b0-45cf-93bf-56df55db8c77"  
            };

            if (protectedUserIds.Contains(id))
            {
                return Forbid("This user cannot be deleted.");
            }
            
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound("User not found");

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded) return BadRequest("Failed to delete user");

            return Ok(new { message = "User deleted successfully" });
        }
    }
}
