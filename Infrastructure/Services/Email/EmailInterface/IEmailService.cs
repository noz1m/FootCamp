namespace Infrastructure.Services.Email.EmailInterface;

public interface IEmailService
{
    Task SendResetPasswordTokenToEmailAsync(string email, string token);
    Task SendEmailConfirmationTokenAsync(string email, string token);
    Task SendBookingReminderAsync(string email, string fullName, string stadiumName, DateTime startTime); //  метод для отправки напоминания о бронировании
}