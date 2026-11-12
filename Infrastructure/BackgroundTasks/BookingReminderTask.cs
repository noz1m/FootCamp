using Infrastructure.Data;
using Infrastructure.Services.Email.EmailInterface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.BackgroundTasks;

public class BookingReminderTask(
    IServiceScopeFactory scopeFactory,
    ILogger<BookingReminderTask> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("BookingReminderTask started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SendRemindersAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred in BookingReminderTask");
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private async Task SendRemindersAsync()
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DataContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        var now = DateTime.UtcNow;
        var reminderFrom = now.AddHours(23);
        var reminderTo = now.AddHours(25);

        var bookings = await context.Bookings
            .Include(b => b.User)
            .Include(b => b.Stadium)
            .Where(b =>
                b.Status == "confirmed" &&
                b.StartTime >= reminderFrom &&
                b.StartTime <= reminderTo)
            .ToListAsync();

        if (!bookings.Any())
        {
            logger.LogInformation("No bookings to remind. Skipping.");
            return;
        }

        foreach (var booking in bookings)
        {
            try
            {
                await emailService.SendBookingReminderAsync(
                    booking.User.Email!,
                    booking.User.FullName,
                    booking.Stadium.Name,
                    booking.StartTime);

                logger.LogInformation(
                    "Reminder sent to {Email} for booking {BookingId}",
                    booking.User.Email, booking.Id);
            }
            catch (Exception ex)
            {
                // Один упавший email не останавливает остальные
                logger.LogError(ex, "Failed to send reminder for booking {BookingId}", booking.Id);
            }
        }

        logger.LogInformation("Processed {Count} booking reminders.", bookings.Count);
    }
}