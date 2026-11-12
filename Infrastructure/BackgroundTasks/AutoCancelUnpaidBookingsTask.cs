using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.BackgroundTasks;

public class AutoCancelUnpaidBookingsTask(
    IServiceScopeFactory scopeFactory,
    ILogger<AutoCancelUnpaidBookingsTask> logger) : BackgroundService
{
    // Время ожидания оплаты — 30 минут после создания бронирования
    private const int PaymentTimeoutMinutes = 30;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("AutoCancelUnpaidBookingsTask started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CancelUnpaidBookingsAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred in AutoCancelUnpaidBookingsTask");
            }

            // Запускаем каждые 5 минут
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }

    private async Task CancelUnpaidBookingsAsync()
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DataContext>();

        var cutoffTime = DateTime.UtcNow.AddMinutes(-PaymentTimeoutMinutes);

        // Ищем бронирования: статус pending, нет оплаченного Payment, созданы давно
        var unpaidBookings = await context.Bookings
            .Include(b => b.Payment)
            .Where(b =>
                b.Status == "pending" &&
                b.CreatedAt <= cutoffTime &&
                (b.Payment == null || b.Payment.Status == "failed"))
            .ToListAsync();

        if (!unpaidBookings.Any())
        {
            logger.LogInformation("No unpaid bookings to cancel. Skipping.");
            return;
        }

        foreach (var booking in unpaidBookings)
        {
            booking.Status = "cancelled";
            booking.UpdatedAt = DateTime.UtcNow;

            logger.LogInformation(
                "Auto-cancelled unpaid booking {BookingId} for user {UserId}. Created at {CreatedAt}",
                booking.Id, booking.UserId, booking.CreatedAt);
        }

        await context.SaveChangesAsync();
        logger.LogInformation("Auto-cancelled {Count} unpaid bookings.", unpaidBookings.Count);
    }
}