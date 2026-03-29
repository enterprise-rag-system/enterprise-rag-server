using System.Collections.Concurrent;
using ApiGateway.Models;

namespace ApiGateway.Stores;

public class InMemoryRateLimitStore: IRateLimitStore
{
    private readonly ConcurrentDictionary<string, TokenBucket> _store = new();

    
    public TokenBucket? Get(string key)
    {
        _store.TryGetValue(key, out var bucket);
        return bucket;
    }

    public void Set(string key, TokenBucket bucket)
    {
        _store[key] = bucket;
    }
}