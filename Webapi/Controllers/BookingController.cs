using Domain.DTOs.BookingDTOs;
using Infrastructure.Bookings.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/bookings")]
[Authorize]
public class BookingController(
    IBookingService bookingService,
    ILogger<BookingController> logger) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        logger.LogInformation("Admin: getting all bookings");
        var result = await bookingService.GetAllBookingsAsync();
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        logger.LogInformation("Getting booking with id: {Id}", id);
        var result = await bookingService.GetBookingByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyBookings()
    {
        logger.LogInformation("Getting bookings for current user");
        var result = await bookingService.GetMyBookingsAsync();
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("my/upcoming")]
    public async Task<IActionResult> GetUpcoming()
    {
        logger.LogInformation("Getting upcoming bookings for current user");
        var result = await bookingService.GetUpcomingBookingsAsync();
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("my/past")]
    public async Task<IActionResult> GetPast()
    {
        logger.LogInformation("Getting past bookings for current user");
        var result = await bookingService.GetPastBookingsAsync();
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("stadium/{stadiumId:guid}")]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<IActionResult> GetByStadium(Guid stadiumId)
    {
        logger.LogInformation("Getting bookings for stadium: {StadiumId}", stadiumId);
        var result = await bookingService.GetBookingsByStadiumIdAsync(stadiumId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("by-status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetByStatus([FromQuery] string status)
    {
        logger.LogInformation("Admin: getting bookings by status: {Status}", status);
        var result = await bookingService.GetBookingsByStatusAsync(status);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("check-availability")]
    [AllowAnonymous]
    public async Task<IActionResult> CheckAvailability(
        [FromQuery] Guid stadiumId,
        [FromQuery] DateTime startTime,
        [FromQuery] DateTime endTime)
    {
        logger.LogInformation("Checking availability for stadium: {StadiumId}", stadiumId);
        var result = await bookingService.CheckAvailabilityAsync(stadiumId, startTime, endTime);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookingDTOs dto)
    {
        logger.LogInformation("Creating booking for stadium: {StadiumId}", dto.StadiumId);
        var result = await bookingService.CreateBookingAsync(dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBookingDTOs dto)
    {
        logger.LogInformation("Updating booking with id: {Id}", id);
        var result = await bookingService.UpdateBookingAsync(id, dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        logger.LogInformation("Cancelling booking with id: {Id}", id);
        var result = await bookingService.CancelBookingAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        logger.LogInformation("Admin: deleting booking with id: {Id}", id);
        var result = await bookingService.DeleteBookingAsync(id);
        return StatusCode(result.StatusCode, result);
    }
}