using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UrlShortener.Core.Entities
{
    public class User
    {

        // Primary key for the User entity
        public Guid Id { get; set; } = Guid.NewGuid();
        // User's email address
        public string Email { get; set; } = string.Empty;
        // Hashed password for the user
        public string PasswordHash { get; set; } = string.Empty;
        // Timestamp for when the user was created
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property for the user's shortened URLs
        public ICollection<Link> Links { get; set; } = new List<Link>();
    }
}