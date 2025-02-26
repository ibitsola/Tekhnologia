using Tekhnologia.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tekhnologia.Services;
using System.Security.Claims;

namespace Tekhnologia.Controllers
{
    [Route("api/digitalresources")]
    [ApiController]
    public class DigitalResourceController : ControllerBase
    {
        private readonly DigitalResourceService _digitalResourceService;

        public DigitalResourceController(DigitalResourceService digitalResourceService)
        {
            _digitalResourceService = digitalResourceService;
        }

        // List all resources with optional filters (category, free/paid)
        [HttpGet]
        [Authorize]
        public IActionResult GetAllResources([FromQuery] string? category, [FromQuery] bool? isFree)
        {
            var resources = _digitalResourceService.GetAllResources(category, isFree);
            return Ok(resources);
        }

        // Admin: Upload a new resource
        [HttpPost("upload")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadResource([FromForm] CreateDigitalResourceDTO resourceDTO)
        {
            try
            {
                string uploaderName = User.Identity?.Name ?? "Unknown";
                var newResource = await _digitalResourceService.UploadResourceAsync(resourceDTO, uploaderName);
                return Ok(new { message = "Resource uploaded successfully!", newResource });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Users: Download a resource (Free or Purchased in the Future)
        [HttpGet("download/{id}")]
        [Authorize]
        public IActionResult DownloadResource(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User ID not found in token");

                var (resource, fileBytes) = _digitalResourceService.DownloadResource(id, userId);
                return File(fileBytes, "application/octet-stream", resource.FileName);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Admin: Delete a resource
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteResource(int id)
        {
            try
            {
                await _digitalResourceService.DeleteResourceAsync(id);
                return Ok("Resource deleted successfully.");
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        // Admin: Edit resource details (title, pricing, category)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditResource(int id, [FromBody] DigitalResourceDTO updatedResource)
        {
            try
            {
                await _digitalResourceService.EditResourceAsync(id, updatedResource);
                return Ok("Resource updated successfully.");
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
