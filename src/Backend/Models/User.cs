using System;
using System.ComponentModel.DataAnnotations; // Required for defining database attributes

namespace Backend.Models
{
    public class User
    {
        [Key] // Marks Guid Id as the Primary Key (Unique Identifier)
        public Guid Id { get; set; } = Guid.NewGuid(); // Unique ID for each user (UUID)

        [Required] // This field is mandatory
        [EmailAddress] // Ensures it's a valid email format
        public string Email { get; set; } = string.Empty;

        [Required] // Must be provided
        public string PasswordHash { get; set; } = string.Empty; // Stores the hashed password (not plain text for security)

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "user"; // Default role is "user", can also be "admin"

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Timestamp when user was created
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow; // Timestamp when user last updated
    }
}
