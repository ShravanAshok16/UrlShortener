using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Api.Models;
using UrlShortener.Core.Entities;
using UrlShortener.Core.Services;
using UrlShortener.Infrastructure.Data;

namespace UrlShortener.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LinksController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly IConfiguration _config;

        public LinksController(AppDbContext dbContext, IConfiguration config)
        {
            _dbContext = dbContext;
            _config = config;
        }

        [HttpPost]
        public async Task<ActionResult<CreateLinkResponse>> CreateLink([FromBody] CreateLinkRequest request)
        {
            var baseUrl = _config["AppSettings:BaseUrl"] ?? "http://localhost:5029";

            var link = new Link
            {
                OriginalUrl = request.OriginalUrl,
                ShortCode = ShortCodeGenerator.Generate(),
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _dbContext.Links.Add(link);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(CreateLink), new CreateLinkResponse
            {
                Id = link.Id,
                OriginalUrl = link.OriginalUrl,
                ShortCode = link.ShortCode,
                ShortUrl = $"{baseUrl}/{link.ShortCode}",
                CreatedAt = link.CreatedAt
            });
        }

        [HttpGet("/{code}")]
        public async Task<IActionResult> RedirectToUrl(string code)
        {
            var link = await _dbContext.Links
                .FirstOrDefaultAsync(l => l.ShortCode == code && l.IsActive);

            if (link is null)
                return NotFound();

            if (link.ExpiresAt.HasValue && link.ExpiresAt < DateTime.UtcNow)
                return NotFound();

            return Redirect(link.OriginalUrl);
        }
    }
}