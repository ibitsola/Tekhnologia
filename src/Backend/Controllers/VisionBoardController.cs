using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/visionboard")]
    public class VisionBoardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public VisionBoardController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Create a new vision board item
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateVisionBoardItem([FromBody] VisionBoardItem item)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized("User ID not found in token");

            item.UserId = userId;
            item.CreatedAt = DateTime.UtcNow;

            _context.VisionBoardItems.Add(item);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Vision board item added successfully", item });
        }

        // Get all vision board items for the logged-in user
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserVisionBoard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized("User ID not found in token");

            var items = await _context.VisionBoardItems
                .Where(v => v.UserId == userId)
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();

            return Ok(items);
        }

        // Get a specific vision board item (only if it belongs to the user)
        [HttpGet("{visionId}")]
        [Authorize]
        public async Task<IActionResult> GetVisionBoardItem(Guid visionId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var item = await _context.VisionBoardItems.FindAsync(visionId);

            if (item == null || item.UserId != userId) return NotFound("Item not found or access denied");

            return Ok(item);
        }

        // Update a vision board item
        [HttpPut("{visionId}")]
        [Authorize]
        public async Task<IActionResult> UpdateVisionBoardItem(Guid visionId, [FromBody] VisionBoardItem updatedItem)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var item = await _context.VisionBoardItems.FindAsync(visionId);

            if (item == null || item.UserId != userId) return NotFound("Item not found or access denied");

            item.ImageUrl = updatedItem.ImageUrl;
            item.Caption = updatedItem.Caption;

            _context.VisionBoardItems.Update(item);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Vision board item updated successfully", item });
        }

        // Delete a vision board item
        [HttpDelete("{visionId}")]
        [Authorize]
        public async Task<IActionResult> DeleteVisionBoardItem(Guid visionId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var item = await _context.VisionBoardItems.FindAsync(visionId);

            if (item == null || item.UserId != userId) return NotFound("Item not found or access denied");

            _context.VisionBoardItems.Remove(item);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Vision board item deleted successfully" });
        }
    }
}
