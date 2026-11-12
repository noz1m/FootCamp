using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.BackgroundTasks;

public class CompletedBookingsUpdateTask(
    IServiceScopeFactory scopeFactory,
    ILogger<CompletedBookingsUpdateTask> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("CompletedBookingsUpdateTask started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await UpdateCompletedBookingsAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred in CompletedBookingsUpdateTask");
            }

            // Запускаем каждые 15 минут
            await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
        }
    }

    private async Task UpdateCompletedBookingsAsync()
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DataContext>();

        var now = DateTime.UtcNow;

        // Бронирования у которых EndTime уже прошёл — переводим в completed
        var expiredBookings = await context.Bookings
            .Where(b =>
                b.Status == "confirmed" &&
                b.EndTime <= now)
            .ToListAsync();

        if (!expiredBookings.Any())
        {
            logger.LogInformation("No bookings to complete. Skipping.");
            return;
        }

        foreach (var booking in expiredBookings)
        {
            booking.Status = "completed";
            booking.UpdatedAt = DateTime.UtcNow;

            logger.LogInformation(
                "Booking {BookingId} marked as completed. EndTime was {EndTime}",
                booking.Id, booking.EndTime);
        }

        await context.SaveChangesAsync();
        logger.LogInformation("Updated {Count} bookings to 'completed'.", expiredBookings.Count);
    }
}