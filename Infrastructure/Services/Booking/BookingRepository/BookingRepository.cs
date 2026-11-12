using Domain.ApiResponse;
using Domain.Entities;
using Domain.Pagination;
using Domain.Filter;
using Infrastructure.Bookings.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Bookings.Repositories;

public class BookingRepository(
    DataContext context,
    ILogger<BookingRepository> logger,
    Pagination<Booking> pagination) : IBookingRepository
{
    public async Task<PagedResponse<Booking>> GetAllAsync(BookingFilter filter)
    {
        try
        {
            var query = context.Bookings
                .Include(b => b.Stadium)
                .Include(b => b.User)
                .Include(b => b.Payment)
                .AsQueryable();

            if (filter.UserId != null)
                query = query.Where(b => b.UserId == filter.UserId);

            if (filter.StadiumId != null)
                query = query.Where(b => b.StadiumId == filter.StadiumId);

            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(b => b.Status.ToLower() == filter.Status.ToLower());

            if (filter.From != null)
                query = query.Where(b => b.StartTime >= filter.From);

            if (filter.To != null)
                query = query.Where(b => b.EndTime <= filter.To);

            return await pagination.GetPagedAsync(query, filter.PageNumber, filter.PageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting all bookings");
            throw;
        }
    }

    public async Task<Booking?> GetByIdAsync(Guid id)
    {
        try
        {
            return await context.Bookings
                .Include(b => b.Stadium)
                .Include(b => b.User)
                .Include(b => b.Payment)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting booking with id: {Id}", id);
            throw;
        }
    }

    // string вместо Guid
    public async Task<List<Booking>> GetByUserIdAsync(string userId)
    {
        try
        {
            return await context.Bookings
                .Include(b => b.Stadium)
                .Include(b => b.Payment)
                .Where(b => b.UserId == userId)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting bookings by userId: {UserId}", userId);
            throw;
        }
    }

    public async Task<List<Booking>> GetByStadiumIdAsync(Guid stadiumId)
    {
        try
        {
            return await context.Bookings
                .Include(b => b.User)
                .Include(b => b.Payment)
                .Where(b => b.StadiumId == stadiumId)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting bookings by stadiumId: {StadiumId}", stadiumId);
            throw;
        }
    }

    public async Task<List<Booking>> GetByStatusAsync(string status)
    {
        try
        {
            return await context.Bookings
                .Include(b => b.Stadium)
                .Include(b => b.User)
                .Where(b => b.Status.ToLower() == status.ToLower())
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting bookings by status: {Status}", status);
            throw;
        }
    }

    // string вместо Guid
    public async Task<List<Booking>> GetUpcomingByUserIdAsync(string userId)
    {
        try
        {
            return await context.Bookings
                .Include(b => b.Stadium)
                .Include(b => b.Payment)
                .Where(b => b.UserId == userId && b.StartTime > DateTime.UtcNow)
                .OrderBy(b => b.StartTime)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting upcoming bookings for userId: {UserId}", userId);
            throw;
        }
    }

    // string вместо Guid
    public async Task<List<Booking>> GetPastByUserIdAsync(string userId)
    {
        try
        {
            return await context.Bookings
                .Include(b => b.Stadium)
                .Include(b => b.Payment)
                .Where(b => b.UserId == userId && b.EndTime < DateTime.UtcNow)
                .OrderByDescending(b => b.StartTime)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting past bookings for userId: {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> IsStadiumAvailableAsync(Guid stadiumId, DateTime startTime, DateTime endTime)
    {
        try
        {
            return !await context.Bookings
                .AnyAsync(b =>
                    b.StadiumId == stadiumId &&
                    b.Status != "cancelled" &&
                    b.StartTime < endTime &&
                    b.EndTime > startTime);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while checking availability for stadiumId: {StadiumId}", stadiumId);
            throw;
        }
    }

    public async Task<int> CreateAsync(Booking booking)
    {
        try
        {
            await context.Bookings.AddAsync(booking);
            return await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while creating booking");
            throw;
        }
    }

    public async Task<int> UpdateAsync(Booking booking)
    {
        try
        {
            booking.UpdatedAt = DateTime.UtcNow;
            context.Bookings.Update(booking);
            return await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while updating booking with id: {Id}", booking.Id);
            throw;
        }
    }

    public async Task<int> DeleteAsync(Booking booking)
    {
        try
        {
            context.Bookings.Remove(booking);
            return await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while deleting booking with id: {Id}", booking.Id);
            throw;
        }
    }
}