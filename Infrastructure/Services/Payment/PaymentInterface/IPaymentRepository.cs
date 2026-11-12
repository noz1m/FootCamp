using Domain.ApiResponse;
using Domain.Entities;
using Domain.Filter;

namespace Infrastructure.Payments.Interfaces;

public interface IPaymentRepository
{
    Task<PagedResponse<Payment>> GetAllAsync(PaymentFilter filter);
    Task<Payment?> GetByIdAsync(Guid id);
    Task<List<Payment>> GetByUserIdAsync(string userId);
    Task<Payment?> GetByBookingIdAsync(Guid bookingId);
    Task<decimal> GetTotalRevenueAsync();
    Task<decimal> GetRevenueByStadiumIdAsync(Guid stadiumId);
    Task<int> CreateAsync(Payment payment);
    Task<int> UpdateAsync(Payment payment);
    Task<int> DeleteAsync(Payment payment);
}