using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Api.Models;
using UrlShortener.Core.Entities;
using UrlShortener.Core.Services;
using UrlShortener.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;

namespace UrlShortener.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]                    // <-- every endpoint requires a token by default
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
        [HttpPost]
        public async Task<ActionResult<CreateLinkResponse>> CreateLink([FromBody] CreateLinkRequest request)
        {
            var userId = GetCurrentUserId();
            var baseUrl = _config["AppSettings:BaseUrl"] ?? "http://localhost:5029";

            var link = new Link
            {
                UserId = userId,                                        // <-- owner
                OriginalUrl = request.OriginalUrl,
                ShortCode = ShortCodeGenerator.Generate(),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = request.ExpiresAt, 
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
                CreatedAt = link.CreatedAt,
                ExpiresAt = link.ExpiresAt
            });
        }

        [HttpGet]
        public async Task<ActionResult<PagedResponse<LinkListItemResponse>>> GetLinks(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool? isActive = null,
            [FromQuery] string? search = null,
            [FromQuery] string sort = "created_desc")
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var userId = GetCurrentUserId();
            var baseUrl = _config["AppSettings:BaseUrl"] ?? "http://localhost:5029";

            var query = _dbContext.Links
                .AsNoTracking()
                .Where(l => l.UserId == userId);

            // Filter: active status
            if (isActive.HasValue)
                query = query.Where(l => l.IsActive == isActive.Value);

            // Filter: search in original URL
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(l => l.OriginalUrl.ToLower().Contains(term));
            }

            // Sort
            query = sort switch
            {
                "created_asc" => query.OrderBy(l => l.CreatedAt),
                "created_desc" => query.OrderByDescending(l => l.CreatedAt),
                "clicks_asc" => query.OrderBy(l => l.Clicks.Count),
                "clicks_desc" => query.OrderByDescending(l => l.Clicks.Count),
                _ => query.OrderByDescending(l => l.CreatedAt)
            };

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
                    ExpiresAt = l.ExpiresAt,
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

        [AllowAnonymous]
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
            var userId = GetCurrentUserId();
            var link = await _dbContext.Links.FindAsync(id);

            if (link is null)
                return NotFound();

            if (link.UserId != userId)
                return NotFound();                            // 404, not 403 — don't leak existence

            link.IsActive = false;
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        private Guid GetCurrentUserId()
        {
            var sub = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
                   ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(sub))
                throw new UnauthorizedAccessException("User identity not found in token.");

            return Guid.Parse(sub);
        }
    }
}