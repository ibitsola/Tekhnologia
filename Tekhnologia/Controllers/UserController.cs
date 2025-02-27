using Tekhnologia.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tekhnologia.Services;
using Tekhnologia.Services.Interfaces;

namespace Tekhnologia.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // Get own profile (any logged-in user)
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found in token");

            var user = await _userService.GetUserProfileAsync(userId);
            if (user == null)
                return NotFound("User not found");

            return Ok(new { user.Id, user.Name, user.Email, user.CreatedAt, user.UpdatedAt });
        }

        // Update own profile (any logged-in user)
        [HttpPut("me")]
        [Authorize]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateUserDTO model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found in token");

            var (success, errors, updatedUser) = await _userService.UpdateUserProfileAsync(userId, model);
            if (!success)
                return BadRequest(errors);

            return Ok(new { message = "Profile updated successfully", user = updatedUser });
        }

        // Change own password
        [HttpPut("me/password")]
        [Authorize]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordDTO model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found in token");

            var (success, errors) = await _userService.UpdateUserPasswordAsync(userId, model);
            if (!success)
                return BadRequest(errors);

            return Ok(new { message = "Password updated successfully. Please log in again." });
        }
    }
}
