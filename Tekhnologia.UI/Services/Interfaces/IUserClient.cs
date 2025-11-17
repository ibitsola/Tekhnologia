using System.Threading.Tasks;

namespace Tekhnologia.UI.Services.Interfaces
{
    public interface IUserClient
    {
        Task<UserInfo?> GetCurrentUserAsync();
    }

    public sealed class UserInfo
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
    }
}