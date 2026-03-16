using Microsoft.Extensions.Caching.Hybrid;
using TeacherSuite.Application.Common.Interfaces;

namespace TeacherSuite.Infrastructure.Caching;

public class CacheService(HybridCache hybridCache) : ICacheService
{
    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? l1Expiration = null,
        TimeSpan? l2Expiration = null,
        IReadOnlyCollection<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        var options = new HybridCacheEntryOptions
        {
            LocalCacheExpiration = l1Expiration ?? TimeSpan.FromMinutes(2),
            Expiration = l2Expiration ?? TimeSpan.FromMinutes(10)
        };

        return await hybridCache.GetOrCreateAsync(
            key,
            async ct => await factory(ct),
            options,
            tags,
            cancellationToken);
    }

    public async Task InvalidateByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        await hybridCache.RemoveByTagAsync(tag, cancellationToken);
    }
}
