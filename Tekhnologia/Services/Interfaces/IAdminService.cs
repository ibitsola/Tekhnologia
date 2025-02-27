using Tekhnologia.Models;
using Tekhnologia.Models.DTOs;

namespace Tekhnologia.Services.Interfaces
{
    public interface IAdminService
    {
        Task<List<object>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(string id);
        Task<(bool Success, IEnumerable<string> Errors, User? UpdatedUser)> UpdateUserAsync(string id, UpdateUserDTO model);
        Task<(bool Success, IEnumerable<string> Errors)> PromoteToAdminAsync(string id);
        Task<(bool Success, IEnumerable<string> Errors)> DeleteUserAsync(string id);
    }
}
