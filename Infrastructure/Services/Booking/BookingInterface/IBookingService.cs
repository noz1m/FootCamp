using Domain.ApiResponse;
using Domain.DTOs.BookingDTOs;

namespace Infrastructure.Bookings.Interfaces;

public interface IBookingService
{
    Task<Response<List<GetBookingDTOs>>> GetAllBookingsAsync();
    Task<Response<GetBookingDTOs>> GetBookingByIdAsync(Guid id);
    Task<Response<List<GetBookingDTOs>>> GetMyBookingsAsync();
    Task<Response<List<GetBookingDTOs>>> GetBookingsByStadiumIdAsync(Guid stadiumId);
    Task<Response<List<GetBookingDTOs>>> GetBookingsByStatusAsync(string status);
    Task<Response<List<GetBookingDTOs>>> GetUpcomingBookingsAsync();
    Task<Response<List<GetBookingDTOs>>> GetPastBookingsAsync();
    Task<Response<GetBookingDTOs>> CreateBookingAsync(CreateBookingDTOs dto);
    Task<Response<GetBookingDTOs>> UpdateBookingAsync(Guid id, UpdateBookingDTOs dto);
    Task<Response<bool>> CancelBookingAsync(Guid id);
    Task<Response<bool>> DeleteBookingAsync(Guid id);
    Task<Response<bool>> CheckAvailabilityAsync(Guid stadiumId, DateTime startTime, DateTime endTime);
}