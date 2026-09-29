using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UrlShortener.Core.Entities
{
    public class Click
    {
        public long Id { get; set; }
        // Foreign key for the link that was clicked
        public Guid LinkId { get; set; }
        // Timestamp for when the click occurred
        public DateTime ClickedAt { get; set; } = DateTime.UtcNow;
        // User agent string of the client that made the click
        public string? UserAgent { get; set; } = string.Empty;
        //Referrer URL of the client that made the click
        public string? Referrer { get; set; } = string.Empty;
        // Hash of the IP address of the client that made the click
        public string? IpHash { get; set; } = string.Empty;

        public Link link { get; set; } = null!;
    }
}