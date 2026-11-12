using System.Net;
using System.Text.Json;

namespace WebApi.Middleware;

public class ErrorHandlingMiddleware(
    RequestDelegate next,
    ILogger<ErrorHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (statusCode, message) = ex switch
        {
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized access."),
            KeyNotFoundException        => (HttpStatusCode.NotFound, "Resource not found."),
            ArgumentException          => (HttpStatusCode.BadRequest, ex.Message),
            InvalidOperationException  => (HttpStatusCode.BadRequest, ex.Message),
            _                          => (HttpStatusCode.InternalServerError, "Internal Server Error.")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            status = (int)statusCode,
            message,
            details = ex.Message
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}