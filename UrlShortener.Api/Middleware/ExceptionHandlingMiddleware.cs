using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace UrlShortener.Api.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var traceId = context.TraceIdentifier;
                _logger.LogError(ex, "Unhandled exception. TraceId: {TraceId}", traceId);

                var (status, code, message) = ex switch
                {
                    UnauthorizedAccessException => (401, "UNAUTHORIZED", "You are not authorized to perform this action."),
                    KeyNotFoundException => (404, "NOT_FOUND", "The requested resource was not found."),
                    _ => (500, "INTERNAL_ERROR", "An unexpected error occurred. Please try again later.")
                };

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = status;

                var response = new
                {
                    error = message,
                    code,
                    traceId,
                    detail = _env.IsDevelopment() ? ex.Message : (string?)null
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        }
    }
}