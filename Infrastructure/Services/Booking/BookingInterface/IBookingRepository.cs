using Domain.ApiResponse;
using Domain.Entities;
using Domain.Filter;

namespace Infrastructure.Bookings.Interfaces;

public interface IBookingRepository
{
    Task<PagedResponse<Booking>> GetAllAsync(BookingFilter filter);
    Task<Booking?> GetByIdAsync(Guid id);
    Task<List<Booking>> GetByUserIdAsync(string userId);
    Task<List<Booking>> GetByStadiumIdAsync(Guid stadiumId);
    Task<List<Booking>> GetByStatusAsync(string status);
    Task<List<Booking>> GetUpcomingByUserIdAsync(string userId);
    Task<List<Booking>> GetPastByUserIdAsync(string userId);
    Task<bool> IsStadiumAvailableAsync(Guid stadiumId, DateTime startTime, DateTime endTime);
    Task<int> CreateAsync(Booking booking);
    Task<int> UpdateAsync(Booking booking);
    Task<int> DeleteAsync(Booking booking);
}