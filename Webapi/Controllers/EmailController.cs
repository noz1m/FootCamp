using Infrastructure.Services.Email.EmailInterface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/email")]
[Authorize(Roles = "Admin")]
public class EmailController(
    IEmailService emailService,
    ILogger<EmailController> logger) : ControllerBase
{
    [HttpPost("send-reset-password")]
    public async Task<IActionResult> SendResetPassword(
        [FromQuery] string email,
        [FromQuery] string token)
    {
        try
        {
            logger.LogInformation("Sending reset password email to: {Email}", email);
            await emailService.SendResetPasswordTokenToEmailAsync(email, token);
            return Ok(new { message = "Reset password email sent successfully." });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send reset password email to: {Email}", email);
            return StatusCode(500, new { message = "Failed to send email.", details = ex.Message });
        }
    }

    [HttpPost("send-confirmation")]
    public async Task<IActionResult> SendConfirmation(
        [FromQuery] string email,
        [FromQuery] string token)
    {
        try
        {
            logger.LogInformation("Sending confirmation email to: {Email}", email);
            await emailService.SendEmailConfirmationTokenAsync(email, token);
            return Ok(new { message = "Confirmation email sent successfully." });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send confirmation email to: {Email}", email);
            return StatusCode(500, new { message = "Failed to send email.", details = ex.Message });
        }
    }

    [HttpPost("send-booking-reminder")]
    public async Task<IActionResult> SendBookingReminder(
        [FromQuery] string email,
        [FromQuery] string fullName,
        [FromQuery] string stadiumName,
        [FromQuery] DateTime startTime)
    {
        try
        {
            logger.LogInformation("Sending booking reminder to: {Email}", email);
            await emailService.SendBookingReminderAsync(email, fullName, stadiumName, startTime);
            return Ok(new { message = "Booking reminder sent successfully." });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send booking reminder to: {Email}", email);
            return StatusCode(500, new { message = "Failed to send email.", details = ex.Message });
        }
    }
}