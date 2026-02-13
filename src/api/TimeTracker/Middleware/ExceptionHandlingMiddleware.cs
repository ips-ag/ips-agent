using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Application.Common.Exceptions;

namespace TimeTracker.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error");
            context.Response.StatusCode = 400;
            context.Response.ContentType = "application/problem+json";
            var problem = new ProblemDetails
            {
                Status = 400,
                Title = "Validation Error",
                Detail = string.Join("; ", ex.Errors.Select(e => e.ErrorMessage)),
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1"
            };
            await context.Response.WriteAsJsonAsync(problem);
        }
        catch (NotFoundException ex)
        {
            context.Response.StatusCode = 404;
            context.Response.ContentType = "application/problem+json";
            var problem = new ProblemDetails
            {
                Status = 404,
                Title = "Not Found",
                Detail = ex.Message,
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.5"
            };
            await context.Response.WriteAsJsonAsync(problem);
        }
        catch (ForbiddenException ex)
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/problem+json";
            var problem = new ProblemDetails
            {
                Status = 403,
                Title = "Forbidden",
                Detail = ex.Message,
            };
            await context.Response.WriteAsJsonAsync(problem);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/problem+json";
            var problem = new ProblemDetails
            {
                Status = 500,
                Title = "Internal Server Error",
                Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1"
            };
            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}
