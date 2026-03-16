using TeacherSuite.Application.Common.Interfaces;

namespace TeacherSuite.Application.Common.Behaviours;

public class CacheInvalidationBehaviour<TRequest, TResponse>(ICacheService cacheService)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICacheInvalidationCommand
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next(cancellationToken);

        foreach (var tag in request.TagsToInvalidate)
        {
            await cacheService.InvalidateByTagAsync(tag, cancellationToken);
        }

        return response;
    }
}
