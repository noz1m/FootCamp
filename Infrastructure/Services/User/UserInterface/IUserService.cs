using Domain.ApiResponse;
using Domain.DTOs.UserDTOs;

namespace Infrastructure.Users.Interfaces;

public interface IUserService
{
    Task<Response<List<GetUserDTO>>> GetAllUsersAsync();
    Task<Response<GetUserDTO>> GetUserByIdAsync(string id);
    Task<Response<GetUserDTO>> GetCurrentUserAsync();
    Task<Response<List<GetUserDTO>>> GetUsersByRoleAsync(string role);
    Task<Response<GetUserDTO>> UpdateUserAsync(string id, UpdateUserDTO dto);
    Task<Response<GetUserDTO>> UpdateMyProfileAsync(UpdateUserDTO dto);
    Task<Response<bool>> DeleteUserAsync(string id);
    Task<Response<bool>> ChangeUserRoleAsync(string userId, string newRole);
    Task<Response<bool>> LockUserAsync(string userId);
    Task<Response<bool>> UnlockUserAsync(string userId);
}