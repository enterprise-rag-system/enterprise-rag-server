namespace ApiGateway.Models;

public class RateLimitingConfig
{
    public RateLimitOptions Global { get; set; } = new();
    public List<EndpointRateLimit> Endpoints { get; set; } = new();
}

public class EndpointRateLimit
{
    public string Path { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public double RefillRate { get; set; }
}
