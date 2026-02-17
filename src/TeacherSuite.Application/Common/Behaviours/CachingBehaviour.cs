using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TeacherSuite.Application.Common.Interfaces;

namespace TeacherSuite.Application.Common.Behaviours;

/// <summary>
/// MediatR pipeline behavior that caches responses for requests implementing <see cref="ICacheableQuery"/>.
/// Always falls back to the handler if the cache is unavailable or the entry is missing.
/// </summary>
public class CachingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachingBehaviour<TRequest, TResponse>> _logger;

    public CachingBehaviour(IMemoryCache cache, ILogger<CachingBehaviour<TRequest, TResponse>> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not ICacheableQuery cacheableQuery)
        {
            return await next();
        }

        var cacheKey = cacheableQuery.CacheKey;

        if (_cache.TryGetValue(cacheKey, out TResponse? cachedResponse) && cachedResponse is not null)
        {
            _logger.LogDebug("Cache hit for key {CacheKey}", cacheKey);
            return cachedResponse;
        }

        _logger.LogDebug("Cache miss for key {CacheKey}. Fetching from source", cacheKey);

        var response = await next();

        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = cacheableQuery.AbsoluteExpirationRelativeToNow,
            Size = cacheableQuery.Size
        };

        _cache.Set(cacheKey, response, cacheOptions);

        return response;
    }
}
