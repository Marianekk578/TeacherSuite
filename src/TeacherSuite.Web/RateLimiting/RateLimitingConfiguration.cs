using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace TeacherSuite.Web.RateLimiting;

public static class RateLimitingServiceExtensions
{
    public static IServiceCollection AddRateLimitingPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        var permitLimit = GetValidatedConfig(configuration, "RateLimiting:PermitLimit", defaultValue: 10);
        var windowSeconds = GetValidatedConfig(configuration, "RateLimiting:WindowSeconds", defaultValue: 60);
        var segmentsPerWindow = GetValidatedConfig(configuration, "RateLimiting:SegmentsPerWindow", defaultValue: 6);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                var partitionKey = GetPartitionKey(httpContext);

                return RateLimitPartition.GetSlidingWindowLimiter(partitionKey, _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = permitLimit,
                    Window = TimeSpan.FromSeconds(windowSeconds),
                    SegmentsPerWindow = segmentsPerWindow,
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

    private static int GetValidatedConfig(IConfiguration configuration, string key, int defaultValue)
    {
        var raw = configuration[key];
        if (raw is null)
            return defaultValue;

        if (!int.TryParse(raw, out var value) || value < 1)
            throw new InvalidOperationException(
                $"Rate limiting configuration '{key}' has an invalid value '{raw}'. Value must be a positive integer.");

        return value;
    }
}
