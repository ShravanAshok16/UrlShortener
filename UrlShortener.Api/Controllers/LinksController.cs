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

        [HttpGet]
        public async Task<ActionResult<PagedResponse<LinkListItemResponse>>> GetLinks(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var baseUrl = _config["AppSettings:BaseUrl"] ?? "http://localhost:5029";

            var query = _dbContext.Links
                .AsNoTracking()
                .OrderByDescending(l => l.CreatedAt);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new LinkListItemResponse
                {
                    Id = l.Id,
                    OriginalUrl = l.OriginalUrl,
                    ShortCode = l.ShortCode,
                    ShortUrl = $"{baseUrl}/{l.ShortCode}",
                    ClickCount = l.Clicks.Count,
                    CreatedAt = l.CreatedAt,
                    IsActive = l.IsActive
                })
                .ToListAsync();

            return Ok(new PagedResponse<LinkListItemResponse>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
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

            var click = new Click
            {
                LinkId = link.Id,
                ClickedAt = DateTime.UtcNow,
                UserAgent = Request.Headers.UserAgent.ToString(),
                Referrer = Request.Headers.Referer.ToString(),
                IpHash = HashIp(HttpContext.Connection.RemoteIpAddress?.ToString())
            };

            _dbContext.Clicks.Add(click);
            await _dbContext.SaveChangesAsync();

            return Redirect(link.OriginalUrl);
        }

        private static string? HashIp(string? ip)
        {
            if (string.IsNullOrEmpty(ip))
                return null;

            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(ip);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToHexString(hash);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteLink(Guid id)
        {
            var link = await _dbContext.Links.FindAsync(id);

            if (link is null)
                return NotFound();

            link.IsActive = false;
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}