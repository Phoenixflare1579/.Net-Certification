using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace UserManagementAPI.Middleware;

public class TokenAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TokenAuthenticationMiddleware> _logger;
    private readonly string? _expectedToken;

    public TokenAuthenticationMiddleware(RequestDelegate next, ILogger<TokenAuthenticationMiddleware> logger, IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _expectedToken = configuration["ApiSettings:Token"];
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only apply to API endpoints
        if (context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrEmpty(_expectedToken))
            {
                _logger.LogWarning("No API token configured. Rejecting request.");
                await ReturnUnauthorized(context, "Unauthorized");
                return;
            }

            if (!context.Request.Headers.TryGetValue("Authorization", out var authHeaderValues))
            {
                await ReturnUnauthorized(context, "Missing Authorization header");
                return;
            }

            var authHeader = authHeaderValues.ToString();
            if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                await ReturnUnauthorized(context, "Invalid Authorization header");
                return;
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();
            if (!string.Equals(token, _expectedToken, StringComparison.Ordinal))
            {
                await ReturnUnauthorized(context, "Invalid token");
                return;
            }

            // token is valid - proceed
        }

        await _next(context);
    }

    private static async Task ReturnUnauthorized(HttpContext context, string message)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";
        var payload = JsonSerializer.Serialize(new { error = "Unauthorized" });
        await context.Response.WriteAsync(payload);
    }
}
