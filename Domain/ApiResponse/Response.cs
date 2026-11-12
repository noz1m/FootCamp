using System.Net;

namespace Domain.ApiResponse;

public class Response<T>
{
    public bool IsSuccess { get; init; }
    public string Message { get; init; }
    public T? Data { get; init; }
    public int StatusCode { get; init; }

    // Main constructor used by all factory methods
    protected Response(bool isSuccess, T? data, string message, HttpStatusCode statusCode)
    {
        IsSuccess = isSuccess;
        Data = data;
        Message = message;
        StatusCode = (int)statusCode;
    }

    // --- SUCCESS METHODS ---

    // Standard 200 OK response
    public static Response<T> Success(T? data, string message = "Request completed successfully.")
        => new(true, data, message, HttpStatusCode.OK);

    // 201 Created response for POST operations
    public static Response<T> Created(T data, string message = "Resource created successfully.")
        => new(true, data, message, HttpStatusCode.Created);

    // --- ERROR METHODS ---

    // Generic failure method for custom status codes
    public static Response<T> Fail(string message, HttpStatusCode statusCode)
        => new(false, default, message, statusCode);

    // 400 Bad Request
    public static Response<T> BadRequest(string message = "Invalid request provided.")
        => Fail(message, HttpStatusCode.BadRequest);

    // 401 Unauthorized
    public static Response<T> Unauthorized(string message = "Authentication is required to access this resource.")
        => Fail(message, HttpStatusCode.Unauthorized);

    // 403 Forbidden
    public static Response<T> Forbidden(string message = "You do not have permission to perform this action.")
        => Fail(message, HttpStatusCode.Forbidden);

    // 404 Not Found
    public static Response<T> NotFound(string message = "The requested resource was not found.")
        => Fail(message, HttpStatusCode.NotFound);

    // 500 Internal Server Error
    public static Response<T> InternalServerError(string message = "An unexpected error occurred on the server.")
        => Fail(message, HttpStatusCode.InternalServerError);
}