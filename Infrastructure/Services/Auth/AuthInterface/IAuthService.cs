using Domain.ApiResponse;
using Domain.DTOs.AuthDTOs;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Services.Auth.AuthInterface;

public interface IAuthService
{
    Task<Response<IdentityResult>> RegisterAsync(RegisterDTOs registerDTOs);
    Task<Response<string>> LoginAsync(LoginDTOs loginDTOs);
    Task<Response<bool>> ChangePasswordAsync(PasswordChangeDTOs passwordChangeDTOs);
    Task<Response<bool>> RequestPasswordResetAsync(string email);
    Task<Response<bool>> ResetAsync(ResetDTOs resetDTOs);
}
