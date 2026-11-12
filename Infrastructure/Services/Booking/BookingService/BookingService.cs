using System.Net;
using System.Security.Claims;
using AutoMapper;
using Domain.ApiResponse;
using Domain.DTOs.BookingDTOs;
using Domain.Entities;
using Domain.Filter;
using Infrastructure.Bookings.Interfaces;
using Infrastructure.Stadiums.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Bookings.Services;

public class BookingService(
    IBookingRepository bookingRepository,
    IStadiumRepository stadiumRepository,
    IHttpContextAccessor httpContextAccessor,
    ILogger<BookingService> logger,
    IMapper mapper) : IBookingService
{
    private string? GetCurrentUserId()
    {
        return httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    public async Task<Response<List<GetBookingDTOs>>> GetAllBookingsAsync()
    {
        try
        {
            var filter = new BookingFilter();  
            var result = await bookingRepository.GetAllAsync(filter);

            var dto = mapper.Map<List<GetBookingDTOs>>(result.Data);
            return Response<List<GetBookingDTOs>>.Success(dto, "Bookings retrieved successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting all bookings");
            return Response<List<GetBookingDTOs>>.Fail("Failed to retrieve bookings.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<GetBookingDTOs>> GetBookingByIdAsync(Guid id)
    {
        try
        {
            var booking = await bookingRepository.GetByIdAsync(id);
            if (booking == null)
                return Response<GetBookingDTOs>.Fail("Booking not found.", HttpStatusCode.NotFound);

            var dto = mapper.Map<GetBookingDTOs>(booking);
            return Response<GetBookingDTOs>.Success(dto, "Booking retrieved successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting booking with id: {Id}", id);
            return Response<GetBookingDTOs>.Fail("Failed to retrieve booking.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<List<GetBookingDTOs>>> GetMyBookingsAsync()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Response<List<GetBookingDTOs>>.Fail("User not found in claims.", HttpStatusCode.Unauthorized);

            var bookings = await bookingRepository.GetByUserIdAsync(userId);

            var dto = mapper.Map<List<GetBookingDTOs>>(bookings);
            return Response<List<GetBookingDTOs>>.Success(dto, "Your bookings retrieved successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting user bookings");
            return Response<List<GetBookingDTOs>>.Fail("Failed to retrieve bookings.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<List<GetBookingDTOs>>> GetBookingsByStadiumIdAsync(Guid stadiumId)
    {
        try
        {
            var bookings = await bookingRepository.GetByStadiumIdAsync(stadiumId);

            var dto = mapper.Map<List<GetBookingDTOs>>(bookings);
            return Response<List<GetBookingDTOs>>.Success(dto, "Stadium bookings retrieved successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting bookings for stadium: {StadiumId}", stadiumId);
            return Response<List<GetBookingDTOs>>.Fail("Failed to retrieve bookings.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<List<GetBookingDTOs>>> GetBookingsByStatusAsync(string status)
    {
        try
        {
            var bookings = await bookingRepository.GetByStatusAsync(status);

            var dto = mapper.Map<List<GetBookingDTOs>>(bookings);
            return Response<List<GetBookingDTOs>>.Success(dto, $"Bookings with status '{status}' retrieved successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting bookings by status: {Status}", status);
            return Response<List<GetBookingDTOs>>.Fail("Failed to retrieve bookings.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<List<GetBookingDTOs>>> GetUpcomingBookingsAsync()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Response<List<GetBookingDTOs>>.Fail("User not found in claims.", HttpStatusCode.Unauthorized);

            var bookings = await bookingRepository.GetUpcomingByUserIdAsync(userId);

            var dto = mapper.Map<List<GetBookingDTOs>>(bookings);
            return Response<List<GetBookingDTOs>>.Success(dto, "Upcoming bookings retrieved successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting upcoming bookings");
            return Response<List<GetBookingDTOs>>.Fail("Failed to retrieve bookings.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<List<GetBookingDTOs>>> GetPastBookingsAsync()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Response<List<GetBookingDTOs>>.Fail("User not found in claims.", HttpStatusCode.Unauthorized);

            var bookings = await bookingRepository.GetPastByUserIdAsync(userId);

            var dto = mapper.Map<List<GetBookingDTOs>>(bookings);
            return Response<List<GetBookingDTOs>>.Success(dto, "Past bookings retrieved successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting past bookings");
            return Response<List<GetBookingDTOs>>.Fail("Failed to retrieve bookings.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<GetBookingDTOs>> CreateBookingAsync(CreateBookingDTOs dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Response<GetBookingDTOs>.Fail("User not found in claims.", HttpStatusCode.Unauthorized);

            // Проверяем что стадион существует
            var stadium = await stadiumRepository.GetByIdAsync(dto.StadiumId);
            if (stadium == null)
                return Response<GetBookingDTOs>.Fail("Stadium not found.", HttpStatusCode.NotFound);

            if (!stadium.IsActive)
                return Response<GetBookingDTOs>.Fail("Stadium is not active.", HttpStatusCode.BadRequest);

            // Проверяем время бронирования
            if (dto.StartTime >= dto.EndTime)
                return Response<GetBookingDTOs>.Fail("End time must be after start time.", HttpStatusCode.BadRequest);

            if (dto.StartTime < DateTime.UtcNow)
                return Response<GetBookingDTOs>.Fail("Cannot book in the past.", HttpStatusCode.BadRequest);

            var isAvailable = await bookingRepository.IsStadiumAvailableAsync(
                dto.StadiumId, dto.StartTime, dto.EndTime);

            if (!isAvailable)
                return Response<GetBookingDTOs>.Fail("Stadium is not available for the selected time.", HttpStatusCode.Conflict);

            var booking = mapper.Map<Booking>(dto);
            booking.Id = Guid.NewGuid();
            booking.UserId = userId;
            booking.Status = "pending";
            booking.CreatedAt = DateTime.UtcNow;

            // Правильный расчёт TotalPrice: часы × цена за час
            var hours = (decimal)(dto.EndTime - dto.StartTime).TotalHours;
            booking.TotalPrice = hours * stadium.PricePerHour;

            await bookingRepository.CreateAsync(booking);

            var result = await bookingRepository.GetByIdAsync(booking.Id);
            var resultDto = mapper.Map<GetBookingDTOs>(result);

            return Response<GetBookingDTOs>.Success(resultDto, "Booking created successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while creating booking");
            return Response<GetBookingDTOs>.Fail("Failed to create booking.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<GetBookingDTOs>> UpdateBookingAsync(Guid id, UpdateBookingDTOs dto)
    {
        try
        {
            var booking = await bookingRepository.GetByIdAsync(id);
            if (booking == null)
                return Response<GetBookingDTOs>.Fail("Booking not found.", HttpStatusCode.NotFound);

            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Response<GetBookingDTOs>.Fail("User not found in claims.", HttpStatusCode.Unauthorized);

            if (booking.UserId != userId)
                return Response<GetBookingDTOs>.Fail("You are not authorized to update this booking.", HttpStatusCode.Forbidden);

            mapper.Map(dto, booking);
            await bookingRepository.UpdateAsync(booking);

            var result = await bookingRepository.GetByIdAsync(id);
            var resultDto = mapper.Map<GetBookingDTOs>(result);

            return Response<GetBookingDTOs>.Success(resultDto, "Booking updated successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while updating booking with id: {Id}", id);
            return Response<GetBookingDTOs>.Fail("Failed to update booking.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<bool>> CancelBookingAsync(Guid id)
    {
        try
        {
            var booking = await bookingRepository.GetByIdAsync(id);
            if (booking == null)
                return Response<bool>.Fail("Booking not found.", HttpStatusCode.NotFound);

            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Response<bool>.Fail("User not found in claims.", HttpStatusCode.Unauthorized);

            if (booking.UserId != userId)
                return Response<bool>.Fail("You are not authorized to cancel this booking.", HttpStatusCode.Forbidden);

            booking.Status = "cancelled";
            await bookingRepository.UpdateAsync(booking);

            return Response<bool>.Success(true, "Booking cancelled successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while cancelling booking with id: {Id}", id);
            return Response<bool>.Fail("Failed to cancel booking.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<bool>> DeleteBookingAsync(Guid id)
    {
        try
        {
            var booking = await bookingRepository.GetByIdAsync(id);
            if (booking == null)
                return Response<bool>.Fail("Booking not found.", HttpStatusCode.NotFound);

            await bookingRepository.DeleteAsync(booking);

            return Response<bool>.Success(true, "Booking deleted successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while deleting booking with id: {Id}", id);
            return Response<bool>.Fail("Failed to delete booking.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<bool>> CheckAvailabilityAsync(Guid stadiumId, DateTime startTime, DateTime endTime)
    {
        try
        {
            var isAvailable = await bookingRepository.IsStadiumAvailableAsync(stadiumId, startTime, endTime);

            return Response<bool>.Success(isAvailable, 
                isAvailable ? "Stadium is available." : "Stadium is not available.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while checking availability for stadium: {StadiumId}", stadiumId);
            return Response<bool>.Fail("Failed to check availability.", HttpStatusCode.InternalServerError);
        }
    }
}