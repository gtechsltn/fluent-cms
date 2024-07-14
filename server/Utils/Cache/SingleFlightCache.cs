using Microsoft.Extensions.Caching.Memory;

namespace Utils.Cache;

//add this to DI
public class MemoryCacheFactory
{
    public MemoryCache Cache { get; } = new MemoryCache(
        new MemoryCacheOptions
        {
            SizeLimit = 1024
        });
}

//asp.net core is going to supports hybrid cache, use memory cache only for now
//https://learn.microsoft.com/en-us/aspnet/core/performance/caching/memory?view=aspnetcore-8.0
//https://learn.microsoft.com/en-us/aspnet/core/performance/caching/hybrid?view=aspnetcore-9.0
public class KeyValCache<T>(IMemoryCache memoryCache, int ttlSeconds)
{
    public async Task<T> GetOrSet(string key,  Func<string, Task<T>> factory)
    {
        return await memoryCache.GetOrCreateAsync<T>(key, async (entry) =>
        {
            entry.SlidingExpiration = TimeSpan.FromSeconds(ttlSeconds);
            return await factory(key);
        })?? await factory(key);
    }
}