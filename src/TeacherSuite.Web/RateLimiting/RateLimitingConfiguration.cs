using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace TeacherSuite.Web.RateLimiting;

public static class RateLimitingServiceExtensions
{
    public static IServiceCollection AddRateLimitingPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                var partitionKey = GetPartitionKey(httpContext);

                return RateLimitPartition.GetSlidingWindowLimiter(partitionKey, _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = int.TryParse(configuration["RateLimiting:PermitLimit"], out var permit) ? permit : 10,
                    Window = TimeSpan.FromSeconds(int.TryParse(configuration["RateLimiting:WindowSeconds"], out var window) ? window : 60),
                    SegmentsPerWindow = int.TryParse(configuration["RateLimiting:SegmentsPerWindow"], out var segments) ? segments : 6,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0,
                });
            });

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.ContentType = "application/problem+json";

                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter =
                        ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString();
                }

                var problemDetails = new ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc6585#section-4",
                    Title = "Too Many Requests",
                    Status = 429,
                    Instance = context.HttpContext.Request.Path,
                    Detail = "Rate limit exceeded. Please try again later.",
                };

                await context.HttpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            };
        });

        return services;
    }

    internal static string GetPartitionKey(HttpContext httpContext)
    {
        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is not null)
            {
                return userId;
            }
        }

        return httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
    }
}
