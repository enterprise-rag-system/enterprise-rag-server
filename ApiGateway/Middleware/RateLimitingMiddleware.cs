using System.Security.Claims;
using ApiGateway.Helpers;
using ApiGateway.Models;
using ApiGateway.Services;
using Microsoft.Extensions.Options;

namespace ApiGateway.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly RateLimitingConfig _config;


    public RateLimitingMiddleware(
        RequestDelegate next,
        IOptions<RateLimitingConfig> config,
        ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
        _config = config.Value;
    }
    
    public async Task InvokeAsync(HttpContext context, IRateLimitService service)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "";

        var userId = GetUserId(context);
        var projectId = GetProjectId(context);

        var options = GetRateLimitOptions(path);

        var key = RateLimitKeyBuilder.Build(userId, projectId, path);

        if (!service.AllowRequest(key, options, out var retryAfter))
        {
            _logger.LogWarning("Rate limit exceeded for {User} on {Path}", userId, path);

            context.Response.StatusCode = 429;
            context.Response.Headers["Retry-After"] = Math.Ceiling(retryAfter).ToString();

            await context.Response.WriteAsJsonAsync(new
            {
                error = "Too many requests"
            });

            return;
        }

        await _next(context);
    }


    private string GetUserId(HttpContext context)
    {
        return context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? context.User?.FindFirst("sub")?.Value
               ?? "anonymous";
    }

    private string GetProjectId(HttpContext context)
    {
        return context.Request.RouteValues["projectId"]?.ToString()
               ?? context.Request.Query["projectId"].ToString()
               ?? "global";
    }

    private RateLimitOptions GetRateLimitOptions(string path)
    {
        // Check endpoint-specific config
        var endpoint = _config.Endpoints
            .FirstOrDefault(e => path.Contains(e.Path.ToLower()));

        if (endpoint != null)
        {
            return new RateLimitOptions
            {
                Capacity = endpoint.Capacity,
                RefillRate = endpoint.RefillRate
            };
        }

        // Fallback to global
        return _config.Global;
    }

}