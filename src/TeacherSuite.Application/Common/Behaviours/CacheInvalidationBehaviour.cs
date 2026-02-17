using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TeacherSuite.Application.Common.Interfaces;

namespace TeacherSuite.Application.Common.Behaviours;

/// <summary>
/// MediatR pipeline behavior that invalidates cached entries after commands
/// implementing <see cref="ICacheInvalidatingCommand"/> succeed.
/// </summary>
public class CacheInvalidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<CacheInvalidationBehaviour<TRequest, TResponse>> _logger;

    public CacheInvalidationBehaviour(IMemoryCache cache, ILogger<CacheInvalidationBehaviour<TRequest, TResponse>> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next();

        if (request is ICacheInvalidatingCommand invalidatingCommand)
        {
            foreach (var key in invalidatingCommand.CacheKeysToInvalidate)
            {
                _logger.LogDebug("Invalidating cache key {CacheKey}", key);
                _cache.Remove(key);
            }
        }

        return response;
    }
}
