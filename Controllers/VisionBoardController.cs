using Data;
using Models;
using Models.DTOs; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Controllers
{
    [ApiController]
    [Route("api/visionboard")] // Base route for vision board APIs
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
        [Authorize] // Ensures only logged-in users can create vision board items
        public async Task<IActionResult> CreateVisionBoardItem([FromBody] CreateVisionBoardItemDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized("User ID not found in token");

            var item = new VisionBoardItem
            {
                UserId = userId,
                ImageUrl = dto.ImageUrl,
                Caption = dto.Caption,
                CreatedAt = DateTime.UtcNow
            };

            _context.VisionBoardItems.Add(item);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Vision board item added successfully", item });
        }

        // Get all vision board items for the logged-in user
        [HttpGet]
        [Authorize] // Ensures only logged-in users can access their vision board
        public async Task<IActionResult> GetUserVisionBoard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized("User ID not found in token");

            var items = await _context.VisionBoardItems
                .Where(v => v.UserId == userId)
                .OrderByDescending(v => v.CreatedAt)
                .Select(v => new VisionBoardItemDTO
                {
                    VisionId = v.VisionId,
                    ImageUrl = v.ImageUrl,
                    Caption = v.Caption,
                    CreatedAt = v.CreatedAt
                })
                .ToListAsync();

            return Ok(items);
        }

        // Update a vision board item
        [HttpPut("{visionId}")]
        [Authorize] // Ensures only the owner can update their vision board items
        public async Task<IActionResult> UpdateVisionBoardItem(Guid visionId, [FromBody] CreateVisionBoardItemDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var item = await _context.VisionBoardItems.FindAsync(visionId);

            if (item == null || item.UserId != userId) return NotFound("Item not found or access denied");

            item.ImageUrl = dto.ImageUrl;
            item.Caption = dto.Caption;

            _context.VisionBoardItems.Update(item);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Vision board item updated successfully", item });
        }

        // Delete a vision board item
        [HttpDelete("{visionId}")]
        [Authorize] // Ensures only the owner can delete their vision board items
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