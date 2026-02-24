using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Moq;
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
        var logger = CreateLogger();
        var environment = CreateEnvironment(Environments.Production);

        var middleware = new GlobalExceptionHandlerMiddleware(
            next: (innerHttpContext) => throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure("Name", "Name is required")
            }),
            logger: logger.Object,
            environment: environment);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal((int)HttpStatusCode.BadRequest, context.Response.StatusCode);
        Assert.Contains("application/json", context.Response.ContentType);

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
        VerifyLoggedError(logger);
    }

    [Fact]
    public async Task NotFoundException_Returns404NotFound_WithProblemDetails()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var logger = CreateLogger();
        var environment = CreateEnvironment(Environments.Production);

        var middleware = new GlobalExceptionHandlerMiddleware(
            next: (innerHttpContext) => throw new Ardalis.GuardClauses.NotFoundException("1", "Entity not found"),
            logger: logger.Object,
            environment: environment);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal((int)HttpStatusCode.NotFound, context.Response.StatusCode);
        Assert.Contains("application/json", context.Response.ContentType);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(problemDetails);
        Assert.Equal(404, problemDetails.Status);
        VerifyLoggedError(logger);
    }

    [Fact]
    public async Task UnhandledException_Returns500InternalServerError_WithProblemDetails()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var logger = CreateLogger();
        var environment = CreateEnvironment(Environments.Production);

        var middleware = new GlobalExceptionHandlerMiddleware(
            next: (innerHttpContext) => throw new InvalidOperationException("Something went wrong"),
            logger: logger.Object,
            environment: environment);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal((int)HttpStatusCode.InternalServerError, context.Response.StatusCode);
        Assert.Contains("application/json", context.Response.ContentType);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(problemDetails);
        Assert.Equal(500, problemDetails.Status);
        VerifyLoggedError(logger);
    }

    [Fact]
    public async Task NotFoundException_Development_IncludesExceptionMessage()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var logger = CreateLogger();
        var environment = CreateEnvironment(Environments.Development);
        var exception = new Ardalis.GuardClauses.NotFoundException("1", "Entity not found");

        var middleware = new GlobalExceptionHandlerMiddleware(
            next: (innerHttpContext) => throw exception,
            logger: logger.Object,
            environment: environment);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(problemDetails);
        Assert.Equal(exception.Message, problemDetails.Detail);
        VerifyLoggedError(logger, exception);
    }

    [Fact]
    public async Task NotFoundException_Production_HidesExceptionMessage()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var logger = CreateLogger();
        var environment = CreateEnvironment(Environments.Production);

        var middleware = new GlobalExceptionHandlerMiddleware(
            next: (innerHttpContext) => throw new Ardalis.GuardClauses.NotFoundException("1", "Entity not found"),
            logger: logger.Object,
            environment: environment);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(problemDetails);
        Assert.Equal("The requested resource was not found.", problemDetails.Detail);
        VerifyLoggedError(logger);
    }

    private static IHostEnvironment CreateEnvironment(string environmentName)
    => new HostingEnvironment { EnvironmentName = environmentName };

    private static Mock<ILogger<GlobalExceptionHandlerMiddleware>> CreateLogger()
        => new Mock<ILogger<GlobalExceptionHandlerMiddleware>>();

    private static void VerifyLoggedError(Mock<ILogger<GlobalExceptionHandlerMiddleware>> logger, Exception? exception = null)
    {
        var expectedException = exception;

        logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) => state.ToString()!.Contains("An exception occurred")),
                It.Is<Exception>(ex => expectedException == null || ex == expectedException),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
