using Domain.ApiResponse;
using Domain.Entities;
using Domain.Pagination;
using Domain.Filter;
using Infrastructure.Data;
using Infrastructure.Payments.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Payments.Repositories;

public class PaymentRepository(
    DataContext context,
    ILogger<PaymentRepository> logger,
    Pagination<Payment> pagination) : IPaymentRepository
{
    public async Task<PagedResponse<Payment>> GetAllAsync(PaymentFilter filter)
    {
        try
        {
            var query = context.Payments
                .Include(p => p.Booking)
                .Include(p => p.User)
                .AsQueryable();

            if (filter.BookingId != null)
                query = query.Where(p => p.BookingId == filter.BookingId);

            if (filter.UserId != null)
                query = query.Where(p => p.UserId == filter.UserId);

            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(p => p.Status.ToLower() == filter.Status.ToLower());

            if (filter.MinAmount != null)
                query = query.Where(p => p.Amount >= filter.MinAmount);

            if (filter.MaxAmount != null)
                query = query.Where(p => p.Amount <= filter.MaxAmount);

            if (filter.From != null)
                query = query.Where(p => p.CreatedAt >= filter.From);

            if (filter.To != null)
                query = query.Where(p => p.CreatedAt <= filter.To);

            return await pagination.GetPagedAsync(query, filter.PageNumber, filter.PageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting all payments");
            throw;
        }
    }

    public async Task<Payment?> GetByIdAsync(Guid id)
    {
        try
        {
            return await context.Payments
                .Include(p => p.Booking)
                .Include(p => p.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting payment with id: {Id}", id);
            throw;
        }
    }

    public async Task<List<Payment>> GetByUserIdAsync(string userId)
    {
        try
        {
            return await context.Payments
                .Include(p => p.Booking)
                .Where(p => p.UserId == userId)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting payments by userId: {UserId}", userId);
            throw;
        }
    }

    public async Task<Payment?> GetByBookingIdAsync(Guid bookingId)
    {
        try
        {
            return await context.Payments
                .Include(p => p.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.BookingId == bookingId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting payment by bookingId: {BookingId}", bookingId);
            throw;
        }
    }

    public async Task<decimal> GetTotalRevenueAsync()
    {
        try
        {
            return await context.Payments
                .Where(p => p.Status == "completed")
                .SumAsync(p => p.Amount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting total revenue");
            throw;
        }
    }

    public async Task<decimal> GetRevenueByStadiumIdAsync(Guid stadiumId)
    {
        try
        {
            return await context.Payments
                .Include(p => p.Booking)
                .Where(p => p.Booking.StadiumId == stadiumId && p.Status == "completed")
                .SumAsync(p => p.Amount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting revenue by stadiumId: {StadiumId}", stadiumId);
            throw;
        }
    }

    public async Task<int> CreateAsync(Payment payment)
    {
        try
        {
            await context.Payments.AddAsync(payment);
            return await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while creating payment");
            throw;
        }
    }

    public async Task<int> UpdateAsync(Payment payment)
    {
        try
        {
            context.Payments.Update(payment);
            return await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while updating payment with id: {Id}", payment.Id);
            throw;
        }
    }

    public async Task<int> DeleteAsync(Payment payment)
    {
        try
        {
            context.Payments.Remove(payment);
            return await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while deleting payment with id: {Id}", payment.Id);
            throw;
        }
    }
}