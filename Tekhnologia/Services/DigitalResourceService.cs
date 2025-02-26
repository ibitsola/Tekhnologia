using Tekhnologia.Data;
using Tekhnologia.Models;
using Tekhnologia.Models.DTOs;
using Tekhnologia.Services.Interfaces; // Add this

namespace Tekhnologia.Services
{
    /// <summary>
    /// Provides business logic for digital resources including listing, uploading,
    /// downloading, deletion, and editing.
    /// </summary>
    public class DigitalResourceService : IDigitalResourceService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public DigitalResourceService(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public List<DigitalResourceDTO> GetAllResources(string? category, bool? isFree)
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

            return resources;
        }

        public async Task<DigitalResource> UploadResourceAsync(CreateDigitalResourceDTO resourceDTO, string uploaderName)
        {
            if (resourceDTO.File == null || resourceDTO.File.Length == 0)
                throw new ArgumentException("File is missing.");

            string uploadPath = Path.Combine(_env.WebRootPath, "digital-resources");
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            string uniqueFileName = $"{Guid.NewGuid()}_{resourceDTO.File.FileName}";
            string filePath = Path.Combine(uploadPath, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await resourceDTO.File.CopyToAsync(stream);
            }

            var newResource = new DigitalResource
            {
                Title = resourceDTO.Title,
                FileName = uniqueFileName,
                FilePath = $"/digital-resources/{uniqueFileName}",
                FileType = Path.GetExtension(resourceDTO.File.FileName).Substring(1),
                IsFree = resourceDTO.IsFree,
                Price = resourceDTO.IsFree ? null : resourceDTO.Price,
                Category = resourceDTO.Category,
                UploadedBy = uploaderName
            };

            _context.DigitalResources.Add(newResource);
            await _context.SaveChangesAsync();
            return newResource;
        }

        public (DigitalResource Resource, byte[] FileBytes) DownloadResource(int id, string userId)
        {
            var resource = _context.DigitalResources.Find(id);
            if (resource == null)
                throw new Exception("Resource not found.");

            if (!resource.IsFree)
            {
                var purchase = _context.Purchases
                    .FirstOrDefault(p => p.DigitalResourceId == id && p.UserId == userId && p.IsPaid);
                if (purchase == null)
                    throw new Exception("Payment required to download this resource.");
            }

            string fullPath = Path.Combine(_env.WebRootPath, "digital-resources", resource.FileName);
            if (!System.IO.File.Exists(fullPath))
                throw new Exception("File does not exist.");

            var fileBytes = System.IO.File.ReadAllBytes(fullPath);
            return (resource, fileBytes);
        }

        public async Task DeleteResourceAsync(int id)
        {
            var resource = _context.DigitalResources.Find(id);
            if (resource == null)
                throw new Exception("Resource not found.");

            string fullPath = Path.Combine(_env.WebRootPath, "digital-resources", resource.FileName);
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);

            _context.DigitalResources.Remove(resource);
            await _context.SaveChangesAsync();
        }

        public async Task EditResourceAsync(int id, DigitalResourceDTO updatedResource)
        {
            var resource = _context.DigitalResources.Find(id);
            if (resource == null)
                throw new Exception("Resource not found.");

            resource.Title = updatedResource.Title;
            resource.IsFree = updatedResource.IsFree;
            resource.Price = updatedResource.IsFree ? null : updatedResource.Price;
            resource.Category = updatedResource.Category;

            await _context.SaveChangesAsync();
        }
    }
}
