using FluentValidation;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using TimeTracker.Application.Common.Exceptions;

namespace TimeTracker.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly ProblemDetailsFactory _problemDetailsFactory;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        ProblemDetailsFactory problemDetailsFactory)
    {
        _next = next;
        _logger = logger;
        _problemDetailsFactory = problemDetailsFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error");
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/problem+json";
            var problem = _problemDetailsFactory.CreateProblemDetails(
                httpContext: context,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Validation Error",
                detail: string.Join("; ", ex.Errors.Select(e => e.ErrorMessage)));
            await context.Response.WriteAsJsonAsync(problem);
        }
        catch (NotFoundException ex)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            context.Response.ContentType = "application/problem+json";
            var problem = _problemDetailsFactory.CreateProblemDetails(
                httpContext: context,
                statusCode: StatusCodes.Status404NotFound,
                title: "Not Found",
                detail: ex.Message);
            await context.Response.WriteAsJsonAsync(problem);
        }
        catch (ForbiddenException ex)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/problem+json";
            var problem = _problemDetailsFactory.CreateProblemDetails(
                httpContext: context,
                statusCode: StatusCodes.Status403Forbidden,
                title: "Forbidden",
                detail: ex.Message);
            await context.Response.WriteAsJsonAsync(problem);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";
            var problem = _problemDetailsFactory.CreateProblemDetails(
                httpContext: context,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Internal Server Error");
            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}
