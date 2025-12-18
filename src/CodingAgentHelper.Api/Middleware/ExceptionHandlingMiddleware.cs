namespace CodingAgentHelper.Api.Middleware;

using CodingAgentHelper.Api.Models;
using System.Net;
using System.Text.Json;

/// <summary>
/// Global exception handling middleware
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    /// <summary>
    /// Initializes the exception handling middleware
    /// </summary>
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware to handle exceptions
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// Handles exceptions and returns appropriate HTTP responses
    /// </summary>
    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ApiErrorResponse
        {
            Timestamp = DateTime.UtcNow,
            TraceId = context.TraceIdentifier
        };

        switch (exception)
        {
            case ArgumentException argEx:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response.StatusCode = 400;
                response.Message = "Invalid argument";
                response.Details = argEx.Message;
                break;

            case KeyNotFoundException notFoundEx:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                response.StatusCode = 404;
                response.Message = "Resource not found";
                response.Details = notFoundEx.Message;
                break;

            case InvalidOperationException invalidOpEx:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response.StatusCode = 400;
                response.Message = "Invalid operation";
                response.Details = invalidOpEx.Message;
                break;

            default:
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                response.StatusCode = 500;
                response.Message = "An unexpected error occurred";
                response.Details = exception.Message;
                break;
        }

        return context.Response.WriteAsJsonAsync(response);
    }
}

/// <summary>
/// Extension methods for adding exception handling middleware
/// </summary>
public static class ExceptionHandlingMiddlewareExtensions
{
    /// <summary>
    /// Adds the exception handling middleware to the pipeline
    /// </summary>
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
