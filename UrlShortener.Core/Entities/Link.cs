using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UrlShortener.Core.Entities
{
    public class Link
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        // Foreign key for the user who created the link
        public Guid? UserId { get; set; }
        // The original URL that is being shortened
        public string OriginalUrl { get; set; } = string.Empty;
        public string ShortCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation property for the user who created the link
        public User? User { get; set; }
        // Navigation property for the clicks associated with the link
        public ICollection<Click> Clicks { get; set; } = new List<Click>();


    }
}