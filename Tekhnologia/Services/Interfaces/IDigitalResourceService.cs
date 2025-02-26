using Tekhnologia.Models;
using Tekhnologia.Models.DTOs;

namespace Tekhnologia.Services.Interfaces
{
    public interface IDigitalResourceService
    {
        List<DigitalResourceDTO> GetAllResources(string? category, bool? isFree);
        Task<DigitalResource> UploadResourceAsync(CreateDigitalResourceDTO resourceDTO, string uploaderName);
        (DigitalResource Resource, byte[] FileBytes) DownloadResource(int id, string userId);
        Task DeleteResourceAsync(int id);
        Task EditResourceAsync(int id, DigitalResourceDTO updatedResource);
    }
}
