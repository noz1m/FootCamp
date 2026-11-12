using System.Net;

namespace Domain.ApiResponse;

public class PagedResponse<T> : Response<IReadOnlyList<T>>
{
    public int TotalRecords { get; init; }
    public int TotalPages { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }

    private PagedResponse(
        IReadOnlyList<T> data,
        int totalRecords,
        int pageNumber,
        int pageSize,
        string message,
        HttpStatusCode statusCode)
        : base(true, data, message, statusCode)
    {
        TotalRecords = totalRecords;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalPages = pageSize > 0
            ? (int)Math.Ceiling(totalRecords / (double)pageSize)
            : 0;
    }

    private PagedResponse(string message, HttpStatusCode statusCode)
        : base(false, null, message, statusCode)
    {
        TotalRecords = 0;
        TotalPages = 0;
        PageNumber = 0;
        PageSize = 0;
    }

    public static PagedResponse<T> Success(
        IReadOnlyList<T> data,
        int totalRecords,
        int pageNumber,
        int pageSize,
        string? message = null,
        HttpStatusCode statusCode = HttpStatusCode.OK)
        => new(data, totalRecords, pageNumber, pageSize,
            message ?? "Request completed successfully.", statusCode);

    public new static PagedResponse<T> Fail(string message, HttpStatusCode statusCode)
        => new(message, statusCode);
}