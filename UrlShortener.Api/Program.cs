using System.Text;
using System.Threading.RateLimiting;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using UrlShortener.Api.Middleware;
using UrlShortener.Api.Services;
using UrlShortener.Api.Validators;
using UrlShortener.Core.Entities;
using UrlShortener.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Custom Services
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<IpHasher>();

// 2. Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

// 3. Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 100,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        });
    });

    options.AddFixedWindowLimiter("auth", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });
});

builder.Services.AddAuthorization();

// 4. Logging
builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration).WriteTo.Console());

// 5. Database
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
}

// 6. CORS (Only define it once, with both URLs)
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "https://url-shortener-ui-flax.vercel.app"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// 7. API Docs & Validation
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateLinkRequestValidator>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

var app = builder.Build();

// 1. Trust the proxy headers (CRITICAL for Render)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
});

// 2. Global Exception & Security Headers
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();

// 3. Logging & HSTS (HSTS enforces HTTPS in the browser)
app.UseSerilogRequestLogging();
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

// ⚠️ REMOVED: app.UseHttpsRedirection(); (Render handles this)

// 4. Routing & CORS
app.UseRouting();
app.UseCors("FrontendPolicy");

// 5. Auth & Rate Limiting
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

// 6. Swagger (Dev only)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 7. Map Endpoints
app.MapControllers();

// Add a health check for Render
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

// 8. Redirect Endpoint (Minimal API)
app.MapGet("/{code:regex(^[[a-zA-Z0-9]]{{7}}$)}", async (string code, AppDbContext db, HttpContext context, IpHasher ipHasher) =>
{
    // Force case-insensitive match just in case
    var link = await db.Links
        .FirstOrDefaultAsync(l => l.ShortCode.ToLower() == code.ToLower() && l.IsActive);

    if (link is null || (link.ExpiresAt.HasValue && link.ExpiresAt < DateTime.UtcNow))
    {
        // Return a custom 404 so we know it hit THIS code block
        return Results.NotFound(new { message = $"Link '{code}' not found in production database." });
    }

    var click = new Click
    {
        LinkId = link.Id,
        ClickedAt = DateTime.UtcNow,
        UserAgent = context.Request.Headers.UserAgent.ToString(),
        Referrer = context.Request.Headers.Referer.ToString(),
        IpHash = ipHasher.Hash(context.Connection.RemoteIpAddress?.ToString())
    };

    db.Clicks.Add(click);
    await db.SaveChangesAsync();

    return Results.Redirect(link.OriginalUrl);
});

app.Run();

public partial class Program { }