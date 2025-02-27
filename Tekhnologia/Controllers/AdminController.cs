using Tekhnologia.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tekhnologia.Services;
using Tekhnologia.Services.Interfaces;

namespace Tekhnologia.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")] // Restrict all routes to Admins
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // Get all users (Admins only)
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var sortedUsers = await _adminService.GetAllUsersAsync();
            return Ok(sortedUsers);
        }

        // Get a user by ID (Admins only)
        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _adminService.GetUserByIdAsync(id);
            if (user == null) return NotFound("User not found");

            return Ok(new { user.Id, user.Name, user.Email, user.CreatedAt, user.UpdatedAt });
        }

        // Update any user (Admins only)
        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDTO model)
        {
            var (success, errors, updatedUser) = await _adminService.UpdateUserAsync(id, model);
            if (!success) return BadRequest(errors);

            return Ok(new { message = $"User {updatedUser?.Name} updated successfully", user = updatedUser });
        }

        // Promote a user to Admin (Admins only)
        [HttpPost("promote/{id}")]
        public async Task<IActionResult> PromoteToAdmin(string id)
        {
            var (success, errors) = await _adminService.PromoteToAdminAsync(id);
            if (!success) return BadRequest(errors);

            return Ok(new { message = "User has been promoted to Admin!" });
        }

       // Delete a user (Admins only)
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var (success, errors) = await _adminService.DeleteUserAsync(id);
            if (!success)
            {
                if (errors.Contains("This user cannot be deleted."))
                    // Return a 403 Forbidden status with a custom message
                    return StatusCode(403, new { message = "This user cannot be deleted." });
                return BadRequest(errors);
            }

            return Ok(new { message = "User deleted successfully" });
        }
    }
}
