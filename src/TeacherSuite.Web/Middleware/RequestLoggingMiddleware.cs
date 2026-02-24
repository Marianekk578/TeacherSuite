using System.Diagnostics;

namespace TeacherSuite.Web.Middleware;

public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = context.TraceIdentifier;

        var sanitizedMethod = SanitizeForLog(context.Request.Method);
        var sanitizedPath = SanitizeForLog(context.Request.Path.ToString());

        logger.LogInformation(
            "Handling request {RequestId}: {Method} {Path} started at {Time}",
            requestId,
            sanitizedMethod,
            sanitizedPath,
            DateTime.UtcNow);

        var sw = Stopwatch.StartNew();

        try
        {
            await next(context);
        }
        finally
        {
            sw.Stop();
            logger.LogInformation(
                "Finished handling request {RequestId}: {Method} {Path} completed at {Time} took {ElapsedMilliseconds} ms",
                requestId,
                sanitizedMethod,
                sanitizedPath,
                DateTime.UtcNow,
                sw.ElapsedMilliseconds);
        }
    }

    /// <summary>
    /// Sanitizes a string for safe logging by removing carriage returns and newlines.
    /// </summary>
    private static string SanitizeForLog(string input)
    {
        if (string.IsNullOrEmpty(input))
            return "";
        return input.Replace("\r", "").Replace("\n", "");
    }
}

public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestLoggingMiddleware>();
    }
}