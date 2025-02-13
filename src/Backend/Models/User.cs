using Microsoft.AspNetCore.Identity;
using System;

namespace Backend.Models
{
    public class User : IdentityUser
    {
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
