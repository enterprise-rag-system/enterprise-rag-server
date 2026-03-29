namespace ApiGateway.Helpers;

public class RateLimitKeyBuilder
{
    public static string Build(string userId, string projectId, string endpoint)
    {
        return $"rate_limit:{userId}:{projectId}:{endpoint}";
    }

}