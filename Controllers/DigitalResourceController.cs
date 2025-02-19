using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.DTOs;

namespace Controllers
{
    [Route("api/digitalresources")]
    [ApiController]
    public class DigitalResourceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public DigitalResourceController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // List all resources with optional filters (category, free/paid)
        [HttpGet]
        public IActionResult GetAllResources([FromQuery] string? category, [FromQuery] bool? isFree)
        {
            var query = _context.DigitalResources.AsQueryable();

            if (!string.IsNullOrEmpty(category))
                query = query.Where(r => r.Category == category);

            if (isFree.HasValue)
                query = query.Where(r => r.IsFree == isFree.Value);

            var resources = query.Select(r => new DigitalResourceDTO
            {
                Id = r.Id,
                Title = r.Title,
                FileType = r.FileType,
                Category = r.Category,
                IsFree = r.IsFree,
                Price = r.Price,
                FilePath = r.FilePath,
                UploadDate = r.UploadDate
            }).ToList();

            return Ok(resources);
        }

        // Admin: Upload a new resource
        [Authorize(Roles = "Admin")]
        [HttpPost("upload")]
        public async Task<IActionResult> UploadResource([FromForm] CreateDigitalResourceDTO resourceDTO)
        {
            if (resourceDTO.File == null || resourceDTO.File.Length == 0)
                return BadRequest("File is missing.");

            string uploadPath = Path.Combine(_env.WebRootPath, "digital-resources");
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            string uniqueFileName = $"{Guid.NewGuid()}_{resourceDTO.File.FileName}";
            string filePath = Path.Combine(uploadPath, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
                await resourceDTO.File.CopyToAsync(stream);

            var newResource = new DigitalResource
            {
                Title = resourceDTO.Title,
                FileName = uniqueFileName,
                FilePath = $"/digital-resources/{uniqueFileName}",
                FileType = Path.GetExtension(resourceDTO.File.FileName).Substring(1),
                IsFree = resourceDTO.IsFree,
                Price = resourceDTO.IsFree ? null : resourceDTO.Price,
                Category = resourceDTO.Category,
                UploadedBy = User.Identity?.Name ?? "Unknown"
            };

            _context.DigitalResources.Add(newResource);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Resource uploaded successfully!", newResource });
        }

        // Users: Download a resource (Free or Purchased in the Future)
        [HttpGet("download/{id}")]
        public IActionResult DownloadResource(int id)
        {
            var resource = _context.DigitalResources.Find(id);
            if (resource == null)
                return NotFound("Resource not found.");

            if (!resource.IsFree)
                return BadRequest("This resource requires a purchase. (Feature Coming Soon)");

            string fullPath = Path.Combine(_env.WebRootPath, "digital-resources", resource.FileName);
            if (!System.IO.File.Exists(fullPath))
                return NotFound("File does not exist.");

            var fileBytes = System.IO.File.ReadAllBytes(fullPath);
            return File(fileBytes, "application/octet-stream", resource.FileName);
        }

        // Admin: Delete a resource
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult DeleteResource(int id)
        {
            var resource = _context.DigitalResources.Find(id);
            if (resource == null)
                return NotFound("Resource not found.");

            string fullPath = Path.Combine(_env.WebRootPath, "digital-resources", resource.FileName);
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);

            _context.DigitalResources.Remove(resource);
            _context.SaveChanges();

            return Ok("Resource deleted successfully.");
        }

        // Admin: Edit resource details (title, pricing, category)
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public IActionResult EditResource(int id, [FromBody] DigitalResourceDTO updatedResource)
        {
            var resource = _context.DigitalResources.Find(id);
            if (resource == null)
                return NotFound("Resource not found.");

            resource.Title = updatedResource.Title;
            resource.IsFree = updatedResource.IsFree;
            resource.Price = updatedResource.IsFree ? null : updatedResource.Price;
            resource.Category = updatedResource.Category;

            _context.SaveChanges();
            return Ok("Resource updated successfully.");
        }
    }
}
