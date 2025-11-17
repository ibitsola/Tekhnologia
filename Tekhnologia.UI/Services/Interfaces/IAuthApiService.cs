using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tekhnologia.UI.Services.Interfaces
{
    public interface IAuthApiService
    {
        Task<(bool Success, IEnumerable<string> Errors)> RegisterAsync(RegisterRequest request);
    }

    public sealed class RegisterRequest
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
    }
}