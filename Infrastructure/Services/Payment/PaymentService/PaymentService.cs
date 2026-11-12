using System.Net;
using System.Security.Claims;
using AutoMapper;
using Domain.ApiResponse;
using Domain.DTOs.PaymentDTOs;
using Domain.Entities;
using Domain.Filter;
using Infrastructure.Payments.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Payments.Services;

public class PaymentService(
    IPaymentRepository paymentRepository,
    IHttpContextAccessor httpContextAccessor,
    ILogger<PaymentService> logger,
    IMapper mapper) : IPaymentService
{
    private string? GetCurrentUserId()
    {
        return httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    public async Task<Response<List<GetPaymentDTOs>>> GetAllPaymentsAsync()
    {
        try
        {
            var filter = new PaymentFilter();
            var result = await paymentRepository.GetAllAsync(filter);

            var dto = mapper.Map<List<GetPaymentDTOs>>(result.Data);
            return Response<List<GetPaymentDTOs>>.Success(dto, "Payments retrieved successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting all payments");
            return Response<List<GetPaymentDTOs>>.Fail("Failed to retrieve payments.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<GetPaymentDTOs>> GetPaymentByIdAsync(Guid id)
    {
        try
        {
            var payment = await paymentRepository.GetByIdAsync(id);
            if (payment == null)
                return Response<GetPaymentDTOs>.Fail("Payment not found.", HttpStatusCode.NotFound);

            var dto = mapper.Map<GetPaymentDTOs>(payment);
            return Response<GetPaymentDTOs>.Success(dto, "Payment retrieved successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting payment with id: {Id}", id);
            return Response<GetPaymentDTOs>.Fail("Failed to retrieve payment.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<List<GetPaymentDTOs>>> GetMyPaymentsAsync()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Response<List<GetPaymentDTOs>>.Fail("User not found in claims.", HttpStatusCode.Unauthorized);

            var payments = await paymentRepository.GetByUserIdAsync(userId);

            var dto = mapper.Map<List<GetPaymentDTOs>>(payments);
            return Response<List<GetPaymentDTOs>>.Success(dto, "Your payments retrieved successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting user payments");
            return Response<List<GetPaymentDTOs>>.Fail("Failed to retrieve payments.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<List<GetPaymentDTOs>>> GetPaymentsByBookingIdAsync(Guid bookingId)
    {
        try
        {
            var payment = await paymentRepository.GetByBookingIdAsync(bookingId);
            var payments = payment != null ? new List<Payment> { payment } : new List<Payment>();

            var dto = mapper.Map<List<GetPaymentDTOs>>(payments);
            return Response<List<GetPaymentDTOs>>.Success(dto, "Booking payments retrieved successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting payments for booking: {BookingId}", bookingId);
            return Response<List<GetPaymentDTOs>>.Fail("Failed to retrieve payments.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<List<GetPaymentDTOs>>> GetPaymentsByStatusAsync(string status)
    {
        try
        {
            var filter = new PaymentFilter { Status = status };
            var result = await paymentRepository.GetAllAsync(filter);

            var dto = mapper.Map<List<GetPaymentDTOs>>(result.Data);
            return Response<List<GetPaymentDTOs>>.Success(dto, $"Payments with status '{status}' retrieved successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting payments by status: {Status}", status);
            return Response<List<GetPaymentDTOs>>.Fail("Failed to retrieve payments.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<GetPaymentDTOs>> CreatePaymentAsync(CreatePaymentDTOs dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Response<GetPaymentDTOs>.Fail("User not found in claims.", HttpStatusCode.Unauthorized);

            var payment = mapper.Map<Payment>(dto);
            payment.Id = Guid.NewGuid();
            payment.UserId = userId;
            payment.Status = "pending";
            payment.CreatedAt = DateTime.UtcNow;

            await paymentRepository.CreateAsync(payment);

            var result = await paymentRepository.GetByIdAsync(payment.Id);
            var resultDto = mapper.Map<GetPaymentDTOs>(result);

            return Response<GetPaymentDTOs>.Success(resultDto, "Payment created successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while creating payment");
            return Response<GetPaymentDTOs>.Fail("Failed to create payment.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<GetPaymentDTOs>> UpdatePaymentAsync(Guid id, UpdatePaymentDTOs dto)
    {
        try
        {
            var payment = await paymentRepository.GetByIdAsync(id);
            if (payment == null)
                return Response<GetPaymentDTOs>.Fail("Payment not found.", HttpStatusCode.NotFound);

            mapper.Map(dto, payment);
            await paymentRepository.UpdateAsync(payment);

            var result = await paymentRepository.GetByIdAsync(id);
            var resultDto = mapper.Map<GetPaymentDTOs>(result);

            return Response<GetPaymentDTOs>.Success(resultDto, "Payment updated successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while updating payment with id: {Id}", id);
            return Response<GetPaymentDTOs>.Fail("Failed to update payment.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<bool>> ProcessPaymentAsync(Guid id)
    {
        try
        {
            var payment = await paymentRepository.GetByIdAsync(id);
            if (payment == null)
                return Response<bool>.Fail("Payment not found.", HttpStatusCode.NotFound);

            payment.Status = "completed";
            payment.CompletedAt = DateTime.UtcNow;
            await paymentRepository.UpdateAsync(payment);

            return Response<bool>.Success(true, "Payment processed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while processing payment with id: {Id}", id);
            return Response<bool>.Fail("Failed to process payment.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<bool>> RefundPaymentAsync(Guid id)
    {
        try
        {
            var payment = await paymentRepository.GetByIdAsync(id);
            if (payment == null)
                return Response<bool>.Fail("Payment not found.", HttpStatusCode.NotFound);

            if (payment.Status != "completed")
                return Response<bool>.Fail("Only completed payments can be refunded.", HttpStatusCode.BadRequest);

            payment.Status = "refunded";
            await paymentRepository.UpdateAsync(payment);

            return Response<bool>.Success(true, "Payment refunded successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while refunding payment with id: {Id}", id);
            return Response<bool>.Fail("Failed to refund payment.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<bool>> DeletePaymentAsync(Guid id)
    {
        try
        {
            var payment = await paymentRepository.GetByIdAsync(id);
            if (payment == null)
                return Response<bool>.Fail("Payment not found.", HttpStatusCode.NotFound);

            await paymentRepository.DeleteAsync(payment);

            return Response<bool>.Success(true, "Payment deleted successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while deleting payment with id: {Id}", id);
            return Response<bool>.Fail("Failed to delete payment.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<decimal>> GetTotalRevenueAsync()
    {
        try
        {
            var revenue = await paymentRepository.GetTotalRevenueAsync();

            return Response<decimal>.Success(revenue, "Total revenue retrieved successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting total revenue");
            return Response<decimal>.Fail("Failed to retrieve total revenue.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<decimal>> GetRevenueByStadiumAsync(Guid stadiumId)
    {
        try
        {
            var revenue = await paymentRepository.GetRevenueByStadiumIdAsync(stadiumId);

            return Response<decimal>.Success(revenue, "Stadium revenue retrieved successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting revenue for stadium: {StadiumId}", stadiumId);
            return Response<decimal>.Fail("Failed to retrieve stadium revenue.", HttpStatusCode.InternalServerError);
        }
    }
}