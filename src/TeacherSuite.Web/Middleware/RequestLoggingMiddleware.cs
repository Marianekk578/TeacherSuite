using System.Diagnostics;

namespace TeacherSuite.Web.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = Guid.NewGuid().ToString();

        _logger.LogInformation(
            "Handling request {RequestId}: {Method} {Path} started at {Time}",
            requestId,
            context.Request.Method,
            context.Request.Path,
            DateTime.UtcNow);

        var sw = Stopwatch.StartNew();

        try
        {
            await _next(context);

        }
        finally
        {
            sw.Stop();
            _logger.LogInformation(
                "Finished handling request {RequestId}: {Method} {Path} completed at {Time} took {ElapsedMilliseconds} ms",
                requestId,
                context.Request.Method,
                context.Request.Path,
                DateTime.UtcNow,
                sw.ElapsedMilliseconds);
        }
    }
}

public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestLoggingMiddleware>();
    }
}