using Domain.DTOs.PaymentDTOs;
using Infrastructure.Payments.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
public class PaymentController(
    IPaymentService paymentService,
    ILogger<PaymentController> logger) : ControllerBase
{
    [Authorize]
    [HttpGet("my")]
    public async Task<IActionResult> GetMyPayments()
    {
        logger.LogInformation("Getting payments for current user");
        var result = await paymentService.GetMyPaymentsAsync();
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        logger.LogInformation("Getting payment with id: {Id}", id);
        var result = await paymentService.GetPaymentByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpGet("booking/{bookingId:guid}")]
    public async Task<IActionResult> GetByBooking(Guid bookingId)
    {
        logger.LogInformation("Getting payments for booking: {BookingId}", bookingId);
        var result = await paymentService.GetPaymentsByBookingIdAsync(bookingId);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePaymentDTOs dto)
    {
        logger.LogInformation("Creating payment for booking: {BookingId}", dto.BookingId);
        var result = await paymentService.CreatePaymentAsync(dto);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePaymentDTOs dto)
    {
        logger.LogInformation("Updating payment with id: {Id}", id);
        var result = await paymentService.UpdatePaymentAsync(id, dto);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpPut("{id:guid}/process")]
    public async Task<IActionResult> Process(Guid id)
    {
        logger.LogInformation("Processing payment with id: {Id}", id);
        var result = await paymentService.ProcessPaymentAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpPut("{id:guid}/refund")]
    public async Task<IActionResult> Refund(Guid id)
    {
        logger.LogInformation("Refunding payment with id: {Id}", id);
        var result = await paymentService.RefundPaymentAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        logger.LogInformation("Deleting payment with id: {Id}", id);
        var result = await paymentService.DeletePaymentAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    // ===== ADMIN =====

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        logger.LogInformation("Admin: getting all payments");
        var result = await paymentService.GetAllPaymentsAsync();
        return StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("by-status")]
    public async Task<IActionResult> GetByStatus([FromQuery] string status)
    {
        logger.LogInformation("Admin: getting payments by status: {Status}", status);
        var result = await paymentService.GetPaymentsByStatusAsync(status);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("revenue/total")]
    public async Task<IActionResult> GetTotalRevenue()
    {
        logger.LogInformation("Admin: getting total revenue");
        var result = await paymentService.GetTotalRevenueAsync();
        return StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpGet("revenue/stadium/{stadiumId:guid}")]
    public async Task<IActionResult> GetRevenueByStadium(Guid stadiumId)
    {
        logger.LogInformation("Getting revenue for stadium: {StadiumId}", stadiumId);
        var result = await paymentService.GetRevenueByStadiumAsync(stadiumId);
        return StatusCode(result.StatusCode, result);
    }
}
