
using ApiGateway.Models;

namespace ApiGateway.Services;

public interface IRateLimitService
{
    bool AllowRequest(string key, RateLimitOptions options, out double retryAfterSeconds);
}

