using Tekhnologia.Models;
using Tekhnologia.Models.DTOs;

namespace Tekhnologia.Services.Interfaces
{
    public interface IVisionBoardService
    {
        Task<VisionBoardItem> CreateVisionBoardItemAsync(string userId, CreateVisionBoardItemDTO dto);
        Task<List<VisionBoardItemDTO>> GetUserVisionBoardAsync(string userId);
        Task<VisionBoardItem?> GetVisionBoardItemByIdAsync(Guid visionId, string userId);
        Task<(bool Success, string Error, VisionBoardItem? UpdatedItem)> UpdateVisionBoardItemAsync(Guid visionId, CreateVisionBoardItemDTO dto, string userId);
        Task<(bool Success, string Error)> DeleteVisionBoardItemAsync(Guid visionId, string userId);
    }
}
