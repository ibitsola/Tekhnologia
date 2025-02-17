using Backend.Models;
using Backend.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;


namespace Backend.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public UserController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // Get all users (admin only) sorted by role and name
        [HttpGet]
        [Authorize(Roles = "Admin")]
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


        // Get users by ID (Admin only)
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userManager.FindByIdAsync(id); 
            if (user == null) return NotFound("User not found");

            return Ok(new
            {
                user.Id,
                user.Name,
                user.Email,
                user.CreatedAt,
                user.UpdatedAt
            });
        }

        // Update any user's profile (Admin only)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
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

        // Post for promote to admin
        [HttpPost("promote/{id}")]
        [Authorize(Roles = "Admin")] // Only Admins can promote users
        public async Task<IActionResult> PromoteToAdmin(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound("User not found");

            if (await _userManager.IsInRoleAsync(user, "Admin"))
                return BadRequest("User is already an Admin");

            await _userManager.AddToRoleAsync(user, "Admin");

            return Ok(new { message = $"{user.Name} has been promoted to Admin!" });
        }

        // Delete user (admin only)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
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

        // Get own profile (any logged-in user) 
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized("User ID not found in token"); 

            var user = await _userManager.FindByIdAsync(userId); 
            if (user == null) return NotFound("User not found");

            return Ok(new
            {
                user.Id,
                user.Name,
                user.Email,
                user.CreatedAt,
                user.UpdatedAt
            });
        }

        // Update own profile (any logged-in user)
        [HttpPut("me")]
        [Authorize]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateUserDTO model)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized("User ID not found in token");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("User not found");

            // Allow users to update only name & email (modify fields as needed)
            user.Name = model.Name ?? user.Name;
            user.Email = model.Email ?? user.Email;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok(new { message = "Profile updated successfully", user });
        }

        [HttpPut("me/password")]
        [Authorize] // Only logged-in users can change their password
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordDTO model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized("User ID not found in token");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("User not found");

            // Check if the old password is correct
            var passwordCheck = await _userManager.CheckPasswordAsync(user, model.OldPassword);
            if (!passwordCheck) return BadRequest("Incorrect old password");

            // Try updating the password
            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (!result.Succeeded) return BadRequest(result.Errors);

            // Force the user to re-authenticate
            await _signInManager.SignOutAsync();

            return Ok(new { message = "Password updated successfully. Please log in again." });
        }
    }
}
