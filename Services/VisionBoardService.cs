using Data;
using Models;
using Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Services
{
    /// <summary>
    /// Provides business logic for creating, retrieving, updating, and deleting vision board items.
    /// </summary>
    public class VisionBoardService
    {
        private readonly ApplicationDbContext _context;

        public VisionBoardService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Creates a new vision board item for the specified user.
        /// </summary>
        public async Task<VisionBoardItem> CreateVisionBoardItemAsync(string userId, CreateVisionBoardItemDTO dto)
        {
            var item = new VisionBoardItem
            {
                UserId = userId,
                ImageUrl = dto.ImageUrl,
                Caption = dto.Caption,
                CreatedAt = DateTime.UtcNow
            };

            _context.VisionBoardItems.Add(item);
            await _context.SaveChangesAsync();
            return item;
        }

        /// <summary>
        /// Retrieves all vision board items for the given user.
        /// </summary>
        public async Task<List<VisionBoardItemDTO>> GetUserVisionBoardAsync(string userId)
        {
            return await _context.VisionBoardItems
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
        }

        /// <summary>
        /// Retrieves a single vision board item by its ID, ensuring it belongs to the specified user.
        /// </summary>
        public async Task<VisionBoardItem?> GetVisionBoardItemByIdAsync(Guid visionId, string userId)
        {
            var item = await _context.VisionBoardItems.FindAsync(visionId);
            if (item == null || item.UserId != userId)
                return null;
            return item;
        }

        /// <summary>
        /// Updates an existing vision board item for the specified user.
        /// </summary>
        public async Task<(bool Success, string Error, VisionBoardItem? UpdatedItem)> UpdateVisionBoardItemAsync(Guid visionId, CreateVisionBoardItemDTO dto, string userId)
        {
            var item = await _context.VisionBoardItems.FindAsync(visionId);
            if (item == null || item.UserId != userId)
                return (false, "Item not found or access denied", null);

            item.ImageUrl = dto.ImageUrl;
            item.Caption = dto.Caption;
            _context.VisionBoardItems.Update(item);
            await _context.SaveChangesAsync();

            return (true, string.Empty, item);
        }

        /// <summary>
        /// Deletes the vision board item for the specified user.
        /// </summary>
        public async Task<(bool Success, string Error)> DeleteVisionBoardItemAsync(Guid visionId, string userId)
        {
            var item = await _context.VisionBoardItems.FindAsync(visionId);
            if (item == null || item.UserId != userId)
                return (false, "Item not found or access denied");

            _context.VisionBoardItems.Remove(item);
            await _context.SaveChangesAsync();

            return (true, string.Empty);
        }
    }
}
