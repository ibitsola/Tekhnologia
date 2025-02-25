using Models;
using Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using System.Security.Claims;

namespace Controllers
{
    [ApiController]
    [Route("api/visionboard")]
    public class VisionBoardController : ControllerBase
    {
        private readonly VisionBoardService _visionBoardService;

        public VisionBoardController(VisionBoardService visionBoardService)
        {
            _visionBoardService = visionBoardService;
        }

        // Create a new vision board item
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateVisionBoardItem([FromBody] CreateVisionBoardItemDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found in token");

            var item = await _visionBoardService.CreateVisionBoardItemAsync(userId, dto);
            return Ok(new { message = "Vision board item added successfully", item });
        }

        // Get all vision board items for the logged-in user
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserVisionBoard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found in token");

            var items = await _visionBoardService.GetUserVisionBoardAsync(userId);
            return Ok(items);
        }

        // Update a vision board item
        [HttpPut("{visionId}")]
        [Authorize]
        public async Task<IActionResult> UpdateVisionBoardItem(Guid visionId, [FromBody] CreateVisionBoardItemDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var (success, error, updatedItem) = await _visionBoardService.UpdateVisionBoardItemAsync(visionId, dto, userId!);
            if (!success)
                return NotFound(error);

            return Ok(new { message = "Vision board item updated successfully", item = updatedItem });
        }

        // Delete a vision board item
        [HttpDelete("{visionId}")]
        [Authorize]
        public async Task<IActionResult> DeleteVisionBoardItem(Guid visionId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var (success, error) = await _visionBoardService.DeleteVisionBoardItemAsync(visionId, userId!);
            if (!success)
                return NotFound(error);

            return Ok(new { message = "Vision board item deleted successfully" });
        }
    }
}
