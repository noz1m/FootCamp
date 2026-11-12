using Domain.ApiResponse;
using Microsoft.EntityFrameworkCore;

namespace Domain.Pagination;

public class Pagination<T>
{
    public async Task<PagedResponse<T>> GetPagedAsync(
        IQueryable<T> query,
        int pageNumber,
        int pageSize)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var totalRecords = await query.CountAsync();

        if (totalRecords == 0)
            return PagedResponse<T>.Success(
                new List<T>(), 0, pageNumber, pageSize, "No records found.");

        var data = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return PagedResponse<T>.Success(data, totalRecords, pageNumber, pageSize);
    }
}