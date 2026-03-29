using ApiGateway.Models;
using ApiGateway.Stores;

namespace ApiGateway.Services;

public class RateLimitService: IRateLimitService
{
    private readonly IRateLimitStore _store;
    
    public RateLimitService(IRateLimitStore store)
    {
        _store = store;
    }

    public bool AllowRequest(string key, RateLimitOptions options, out double retryAfterSeconds)
    {
        var now = DateTime.UtcNow;

        var bucket = _store.Get(key);

        if (bucket == null)
        {
            bucket = new TokenBucket
            {
                Tokens = options.Capacity,
                LastRefill = now
            };
        }

        // Refill tokens
        var elapsedSeconds = (now - bucket.LastRefill).TotalSeconds;
        var tokensToAdd = elapsedSeconds * options.RefillRate;

        bucket.Tokens = Math.Min(options.Capacity, bucket.Tokens + tokensToAdd);
        bucket.LastRefill = now;

        if (bucket.Tokens >= 1)
        {
            bucket.Tokens -= 1;
            _store.Set(key, bucket);
            retryAfterSeconds = 0;
            return true;
        }

        retryAfterSeconds = (1 - bucket.Tokens) / options.RefillRate;
        _store.Set(key, bucket);

        return false;
    } 
}
