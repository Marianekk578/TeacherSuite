using Microsoft.AspNetCore.Mvc;
using System.Net;
using TeacherSuite.Application.Common;

namespace TeacherSuite.Web.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, problemDetails) = exception switch
        {
            ValidationException validationException => (
                HttpStatusCode.BadRequest,
                CreateValidationProblemDetails(context, validationException)
            ),
            Ardalis.GuardClauses.NotFoundException notFoundException => (
                HttpStatusCode.NotFound,
                CreateNotFoundProblemDetails(context, notFoundException)
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                CreateInternalServerErrorProblemDetails(context, exception)
            )
        };

        _logger.LogError(exception, "An exception occurred: {Message}", exception.Message);

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(problemDetails);
    }

    private static ProblemDetails CreateValidationProblemDetails(
        HttpContext context,
        ValidationException exception)
    {
        var problemDetails = new ValidationProblemDetails(exception.Errors)
        {
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            Title = "One or more validation errors occurred.",
            Status = (int)HttpStatusCode.BadRequest,
            Instance = context.Request.Path,
            Detail = "Please refer to the errors property for additional details."
        };

        return problemDetails;
    }

    private static ProblemDetails CreateNotFoundProblemDetails(
        HttpContext context,
        Ardalis.GuardClauses.NotFoundException exception)
    {
        var problemDetails = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
            Title = "The specified resource was not found.",
            Status = (int)HttpStatusCode.NotFound,
            Instance = context.Request.Path,
            Detail = exception.Message
        };

        return problemDetails;
    }

    private static ProblemDetails CreateInternalServerErrorProblemDetails(
        HttpContext context,
        Exception exception)
    {
        var problemDetails = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
            Title = "An error occurred while processing your request.",
            Status = (int)HttpStatusCode.InternalServerError,
            Instance = context.Request.Path,
            Detail = "An unexpected error occurred. Please try again later."
        };

        return problemDetails;
    }
}
