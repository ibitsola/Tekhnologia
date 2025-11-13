using Tekhnologia.Models;
using Tekhnologia.Models.DTOs;

namespace Tekhnologia.Services.Interfaces
{
    public interface IUserService
    {
        Task<User?> GetUserProfileAsync(string userId);
        Task<(bool Success, IEnumerable<string> Errors, User? UpdatedUser)> UpdateUserProfileAsync(string userId, UpdateUserDTO model);
        Task<(bool Success, IEnumerable<string> Errors)> UpdateUserPasswordAsync(string userId, UpdatePasswordDTO model);

        Task<User?> GetCurrentUserAsync();
    }
}
