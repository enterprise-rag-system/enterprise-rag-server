using ApiGateway.Models;

namespace ApiGateway.Stores;

public interface IRateLimitStore
{
    TokenBucket? Get(string key);
    void Set(string key, TokenBucket bucket);
}