using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Domain.ApiResponse;
using Domain.DTOs.AuthDTOs;
using Infrastructure.Services.Auth.AuthInterface;
using Infrastructure.Services.Email.EmailInterface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.Net;
using Domain.Entities;

namespace Infrastructure.Services.Auth.AuthService;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration,
    IHttpContextAccessor httpContextAccessor,
    IEmailService emailService) : IAuthService
{
    private async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var keyValue = configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(keyValue))
            throw new InvalidOperationException("JWT key is not configured in appsettings.json");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyValue));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<Response<IdentityResult>> RegisterAsync(RegisterDTOs registerDTOs)
    {
        var existingUser = await userManager.FindByEmailAsync(registerDTOs.Email);
        if (existingUser != null)
            return Response<IdentityResult>.Fail("User with this email already exists.", HttpStatusCode.Conflict);

        var user = new ApplicationUser
        {
            UserName = registerDTOs.Email,
            Email = registerDTOs.Email,
            PhoneNumber = registerDTOs.PhoneNumber,
            FullName = registerDTOs.FullName,
            CreatedAt = DateTime.UtcNow,
            EmailConfirmed = true  // подтверждаем сразу — без SMTP
        };

        var result = await userManager.CreateAsync(user, registerDTOs.Password);
        if (!result.Succeeded)
            return Response<IdentityResult>.Fail(
                string.Join(", ", result.Errors.Select(e => e.Description)),
                HttpStatusCode.BadRequest);

        var roleResult = await userManager.AddToRoleAsync(user, "User");
        if (!roleResult.Succeeded)
            return Response<IdentityResult>.Fail(
                "User created but failed to assign role.",
                HttpStatusCode.InternalServerError);

        return Response<IdentityResult>.Success(result, "User registered successfully.");
    }

    public async Task<Response<string>> LoginAsync(LoginDTOs loginDTOs)
    {
        var user = await userManager.FindByEmailAsync(loginDTOs.Email);
        if (user == null)
            return Response<string>.Fail("Invalid email or password.", HttpStatusCode.Unauthorized);

        var validPassword = await userManager.CheckPasswordAsync(user, loginDTOs.Password);
        if (!validPassword)
            return Response<string>.Fail("Invalid email or password.", HttpStatusCode.Unauthorized);

        var token = await GenerateJwtTokenAsync(user);
        return Response<string>.Success(token, "Login successful.");
    }

    public async Task<Response<bool>> ChangePasswordAsync(PasswordChangeDTOs passwordChangeDTOs)
    {
        var userEmail = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(userEmail))
            return Response<bool>.Fail("User email not found in claims.", HttpStatusCode.Unauthorized);

        var user = await userManager.FindByEmailAsync(userEmail);
        if (user == null)
            return Response<bool>.Fail("User not found.", HttpStatusCode.NotFound);

        var result = await userManager.ChangePasswordAsync(
            user, passwordChangeDTOs.OldPassword, passwordChangeDTOs.NewPassword);

        return result.Succeeded
            ? Response<bool>.Success(true, "Password changed successfully.")
            : Response<bool>.Fail(
                string.Join(", ", result.Errors.Select(e => e.Description)),
                HttpStatusCode.BadRequest);
    }

    public async Task<Response<bool>> RequestPasswordResetAsync(string email)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
            return Response<bool>.Success(true, "If the email exists, a reset link has been sent.");

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        await emailService.SendResetPasswordTokenToEmailAsync(email, token);

        return Response<bool>.Success(true, "Password reset token sent to email.");
    }

    public async Task<Response<bool>> ResetAsync(ResetDTOs resetDTOs)
    {
        var user = await userManager.FindByEmailAsync(resetDTOs.Email);
        if (user == null)
            return Response<bool>.Fail("Invalid reset token or email.", HttpStatusCode.BadRequest);

        var result = await userManager.ResetPasswordAsync(
            user, resetDTOs.Token, resetDTOs.NewPassword);

        return result.Succeeded
            ? Response<bool>.Success(true, "Password has been reset successfully.")
            : Response<bool>.Fail(
                string.Join(", ", result.Errors.Select(e => e.Description)),
                HttpStatusCode.BadRequest);
    }
}