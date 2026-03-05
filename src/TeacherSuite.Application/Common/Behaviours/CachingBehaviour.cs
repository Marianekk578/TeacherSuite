using TeacherSuite.Application.Common.Interfaces;

namespace TeacherSuite.Application.Common.Behaviours;

public class CachingBehaviour<TRequest, TResponse>(ICacheService cacheService)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICacheableQuery
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        return await cacheService.GetOrCreateAsync(
            request.CacheKey,
            async ct =>
            {
                var response = await next();
                return response;
            },
            request.L1Expiration,
            request.L2Expiration,
            request.Tags,
            cancellationToken);
    }
}
