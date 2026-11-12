using Domain.DTOs.AuthDTOs;
using Infrastructure.Services.Auth.AuthInterface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    IAuthService authService,
    ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDTOs dto)
    {
        logger.LogInformation("Register attempt for email: {Email}", dto.Email);
        var result = await authService.RegisterAsync(dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTOs dto)
    {
        logger.LogInformation("Login attempt for email: {Email}", dto.Email);
        var result = await authService.LoginAsync(dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromQuery] string email)
    {
        logger.LogInformation("Password reset request for email: {Email}", email);
        var result = await authService.RequestPasswordResetAsync(email);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetDTOs dto)
    {
        logger.LogInformation("Password reset for email: {Email}", dto.Email);
        var result = await authService.ResetAsync(dto);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] PasswordChangeDTOs dto)
    {
        logger.LogInformation("Change password request");
        var result = await authService.ChangePasswordAsync(dto);
        return StatusCode(result.StatusCode, result);
    }
}