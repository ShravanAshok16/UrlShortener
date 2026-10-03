using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UrlShortener.Api.Models
{
    public class CreateLinkRequest
    {
        public string OriginalUrl { get; set; } = string.Empty;
    }
}