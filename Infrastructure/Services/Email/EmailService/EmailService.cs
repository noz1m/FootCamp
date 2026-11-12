using System.Net;
using System.Net.Mail;
using Infrastructure.Services.Email.EmailInterface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Email.EmailService;

public class EmailService(
    IConfiguration configuration,
    ILogger<EmailService> logger) : IEmailService
{
    private readonly string _fromEmail = "nozimqcsgo@gmail.com";

    public async Task SendResetPasswordTokenToEmailAsync(string email, string token)
    {
        try
        {
            var fromPassword = configuration["EmailSettings:Password"];
            if (string.IsNullOrEmpty(fromPassword))
            {
                logger.LogError("Email password is not configured in appsettings.json");
                throw new InvalidOperationException("Email settings are not properly configured.");
            }

            var subject = "Password Reset Request";
            var body = $@"
                <html>
                <body>
                    <h2>Password Reset Request</h2>
                    <p>You have requested to reset your password.</p>
                    <p>Please use the following token to reset your password:</p>
                    <p><strong>{token}</strong></p>
                    <p>If you did not request this, please ignore this email.</p>
                    <br>
                    <p>Best regards,<br>FootCamp Team</p>
                </body>
                </html>
            ";

            await SendEmailAsync(email, subject, body, true);
            logger.LogInformation("Password reset email sent to {Email}", email);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending password reset email to {Email}", email);
            throw;
        }
    }

    public async Task SendEmailConfirmationTokenAsync(string email, string token)
    {
        try
        {
            var fromPassword = configuration["EmailSettings:Password"];
            if (string.IsNullOrEmpty(fromPassword))
            {
                logger.LogError("Email password is not configured in appsettings.json");
                throw new InvalidOperationException("Email settings are not properly configured.");
            }

            var subject = "Confirm Your Email Address";
            var body = $@"
                <html>
                <body>
                    <h2>Welcome to FootCamp!</h2>
                    <p>Thank you for registering with us.</p>
                    <p>Please use the following token to confirm your email address:</p>
                    <p><strong>{token}</strong></p>
                    <p>If you did not create this account, please ignore this email.</p>
                    <br>
                    <p>Best regards,<br>FootCamp Team</p>
                </body>
                </html>
            ";

            await SendEmailAsync(email, subject, body, true);
            logger.LogInformation("Email confirmation sent to {Email}", email);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending email confirmation to {Email}", email);
            throw;
        }
    }

    private async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = false)
    {
        var fromPassword = configuration["EmailSettings:Password"];
        
        using var smtpClient = new SmtpClient
        {
            Host = "smtp.gmail.com",
            Port = 587,
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(_fromEmail, fromPassword)
        };

        using var message = new MailMessage(_fromEmail, toEmail, subject, body)
        {
            IsBodyHtml = isHtml
        };

        await smtpClient.SendMailAsync(message);
    }
    public async Task SendBookingReminderAsync(string email, string fullName, string stadiumName, DateTime startTime)
{
    try
    {
        var subject = "Reminder: Your Booking Tomorrow";
        var body = $@"
            <html>
            <body>
                <h2>Booking Reminder</h2>
                <p>Hello, <strong>{fullName}</strong>!</p>
                <p>This is a reminder that you have a booking tomorrow:</p>
                <ul>
                    <li><strong>Stadium:</strong> {stadiumName}</li>
                    <li><strong>Start Time:</strong> {startTime:dd MMM yyyy, HH:mm} UTC</li>
                </ul>
                <p>Please make sure to arrive on time.</p>
                <br>
                <p>Best regards,<br>StadiumHub Team</p>
            </body>
            </html>
        ";

        await SendEmailAsync(email, subject, body, true);
        logger.LogInformation("Booking reminder sent to {Email}", email);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error sending booking reminder to {Email}", email);
        throw;
    }
}
}