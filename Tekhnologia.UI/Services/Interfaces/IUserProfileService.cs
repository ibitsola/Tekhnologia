using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tekhnologia.UI.Services.Interfaces
{
    public interface IUserProfileService
    {
        Task<UserInfo?> GetProfileAsync(string userId);
        Task<(bool Success, IEnumerable<string> Errors)> UpdateNameAsync(string userId, string name);
        Task<(bool Success, IEnumerable<string> Errors)> UpdatePasswordAsync(string userId, string oldPassword, string newPassword);
    }
}