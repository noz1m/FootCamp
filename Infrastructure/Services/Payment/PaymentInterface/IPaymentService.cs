using Domain.ApiResponse;
using Domain.DTOs.PaymentDTOs;

namespace Infrastructure.Payments.Interfaces;

public interface IPaymentService
{
    Task<Response<List<GetPaymentDTOs>>> GetAllPaymentsAsync();
    Task<Response<GetPaymentDTOs>> GetPaymentByIdAsync(Guid id);
    Task<Response<List<GetPaymentDTOs>>> GetMyPaymentsAsync();
    Task<Response<List<GetPaymentDTOs>>> GetPaymentsByBookingIdAsync(Guid bookingId);
    Task<Response<List<GetPaymentDTOs>>> GetPaymentsByStatusAsync(string status);
    Task<Response<GetPaymentDTOs>> CreatePaymentAsync(CreatePaymentDTOs dto);
    Task<Response<GetPaymentDTOs>> UpdatePaymentAsync(Guid id, UpdatePaymentDTOs dto);
    Task<Response<bool>> ProcessPaymentAsync(Guid id);
    Task<Response<bool>> RefundPaymentAsync(Guid id);
    Task<Response<bool>> DeletePaymentAsync(Guid id);
    Task<Response<decimal>> GetTotalRevenueAsync();
    Task<Response<decimal>> GetRevenueByStadiumAsync(Guid stadiumId);
}