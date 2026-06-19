using System.Threading.RateLimiting;

namespace TeacherSuite.Web.Middleware;

public static class RateLimitingMiddlewareExtensions
{
    private static readonly TimeSpan Window = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan DefaultRetryAfter = TimeSpan.FromSeconds(10);

    public static IServiceCollection AddWebApiRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                var partitionKey = httpContext.User.Identity?.Name;
                if (string.IsNullOrWhiteSpace(partitionKey))
                {
                    partitionKey = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown-ip";
                }

                return RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey,
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = Window,
                        SegmentsPerWindow = 6,
                        QueueLimit = 0,
                        AutoReplenishment = true,
                    });
            });

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/json";

                var retryAfter = DefaultRetryAfter;
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterMetadata))
                {
                    retryAfter = retryAfterMetadata;
                }

                context.HttpContext.Response.Headers.RetryAfter = Math.Ceiling(retryAfter.TotalSeconds).ToString();

                await context.HttpContext.Response.WriteAsJsonAsync(
                    new
                    {
                        status = StatusCodes.Status429TooManyRequests,
                        title = "Too many requests",
                        detail = "Rate limit exceeded. Please retry after the number of seconds specified in the Retry-After header.",
                    },
                    cancellationToken);
            };
        });

        return services;
    }

    public static IApplicationBuilder UseWebApiRateLimiting(this IApplicationBuilder app)
    {
        return app.UseRateLimiter();
    }
}
