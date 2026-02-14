using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using TeacherSuite.Application.Common;
using TeacherSuite.Web.Middleware;

namespace Application.UnitTests;

public class GlobalExceptionHandlerMiddlewareTests
{
    [Fact]
    public async Task ValidationException_Returns400BadRequest_WithProblemDetails()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var logger = new TestLogger<GlobalExceptionHandlerMiddleware>();
        
        var middleware = new GlobalExceptionHandlerMiddleware(
            next: (innerHttpContext) => throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure("Name", "Name is required")
            }),
            logger: logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal((int)HttpStatusCode.BadRequest, context.Response.StatusCode);
        Assert.Contains("application/problem+json", context.Response.ContentType);
        
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<ValidationProblemDetails>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        Assert.NotNull(problemDetails);
        Assert.Equal(400, problemDetails.Status);
        Assert.NotNull(problemDetails.Errors);
        Assert.True(problemDetails.Errors.Count > 0, $"Expected at least one error. Response: {responseBody}");
    }

    [Fact]
    public async Task NotFoundException_Returns404NotFound_WithProblemDetails()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var logger = new TestLogger<GlobalExceptionHandlerMiddleware>();
        
        var middleware = new GlobalExceptionHandlerMiddleware(
            next: (innerHttpContext) => throw new Ardalis.GuardClauses.NotFoundException("1", "Entity not found"),
            logger: logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal((int)HttpStatusCode.NotFound, context.Response.StatusCode);
        Assert.Contains("application/problem+json", context.Response.ContentType);
        
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        Assert.NotNull(problemDetails);
        Assert.Equal(404, problemDetails.Status);
    }

    [Fact]
    public async Task UnhandledException_Returns500InternalServerError_WithProblemDetails()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var logger = new TestLogger<GlobalExceptionHandlerMiddleware>();
        
        var middleware = new GlobalExceptionHandlerMiddleware(
            next: (innerHttpContext) => throw new InvalidOperationException("Something went wrong"),
            logger: logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal((int)HttpStatusCode.InternalServerError, context.Response.StatusCode);
        Assert.Contains("application/problem+json", context.Response.ContentType);
        
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        Assert.NotNull(problemDetails);
        Assert.Equal(500, problemDetails.Status);
    }
}

internal class TestLogger<T> : ILogger<T>
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
}
