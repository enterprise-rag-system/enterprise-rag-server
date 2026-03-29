namespace ApiGateway.Models;

public class RateLimitOptions
{
    public int Capacity { get; set; }       
    public double RefillRate { get; set; }  
}
