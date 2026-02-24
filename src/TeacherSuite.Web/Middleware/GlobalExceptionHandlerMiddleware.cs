using Microsoft.AspNetCore.Mvc;
using TeacherSuite.Application.Common;

namespace TeacherSuite.Web.Middleware;

public class GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger, IHostEnvironment environment)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        logger.LogError(exception, "An exception occurred: {Message}", exception.Message);

        if (context.Response.HasStarted)
        {
            logger.LogWarning("The response has already started, the global exception handler will not modify the response.");
            return;
        }
        context.Response.Clear();

        context.Response.ContentType = "application/problem+json";

        switch (exception)
        {
            case ValidationException validationException:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(CreateValidationProblemDetails(context, validationException));
                break;

            case Ardalis.GuardClauses.NotFoundException notFoundException:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                await context.Response.WriteAsJsonAsync(CreateNotFoundProblemDetails(context, notFoundException, environment));
                break;

            case ConflictException conflictException:
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                await context.Response.WriteAsJsonAsync(CreateConflictProblemDetails(context, conflictException));
                break;

            default:
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(CreateInternalServerErrorProblemDetails(context, exception));
                break;
        }
    }

    private ValidationProblemDetails CreateValidationProblemDetails(
        HttpContext context,
        ValidationException exception)
    {
        var problemDetails = new ValidationProblemDetails(exception.Errors)
        {
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            Title = "One or more validation errors occurred.",
            Status = StatusCodes.Status400BadRequest,
            Instance = context.Request.Path,
            Detail = "Please refer to the errors property for additional details."
        };

        return problemDetails;
    }

    private ProblemDetails CreateNotFoundProblemDetails(
        HttpContext context,
        Ardalis.GuardClauses.NotFoundException exception, 
        IHostEnvironment environment)
    {
        var isDevelopment = environment.IsDevelopment();
        
        var problemDetails = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
            Title = "The specified resource was not found.",
            Status = StatusCodes.Status404NotFound,
            Instance = context.Request.Path,
            Detail = isDevelopment ? exception.Message : "The requested resource was not found."
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
            Status = StatusCodes.Status500InternalServerError,
            Instance = context.Request.Path,
            Detail = "An unexpected error occurred. Please try again later."
        };

        return problemDetails;
    }

    private static ProblemDetails CreateConflictProblemDetails(
        HttpContext context,
        ConflictException exception)
    {
        var problemDetails = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
            Title = "The request could not be completed due to a conflict with the current state of the resource.",
            Status = StatusCodes.Status409Conflict,
            Instance = context.Request.Path,
            Detail = exception.Message
        };

        return problemDetails;
    }
}
