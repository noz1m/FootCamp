using System.Net;
using System.Security.Claims;
using AutoMapper;
using Domain.ApiResponse;
using Domain.DTOs.UserDTOs;
using Domain.Entities;
using Domain.Filter;
using Infrastructure.RedisCache;
using Infrastructure.RedisCache.RedisCacheService;
using Infrastructure.Users.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Users.Services;

public class UserService(
    IUserRepository userRepository,
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor httpContextAccessor,
    IRedisCacheService cache,
    ILogger<UserService> logger,
    IMapper mapper) : IUserService
{
    private string? GetCurrentUserId()
    {
        return httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    // ===== Вспомогательный метод: маппинг + подгрузка роли =====
    private async Task<GetUserDTO> MapWithRoleAsync(ApplicationUser user)
    {
        var dto = mapper.Map<GetUserDTO>(user);
        var roles = await userManager.GetRolesAsync(user);
        dto.Role = roles.FirstOrDefault() ?? "User";
        return dto;
    }

    private async Task<List<GetUserDTO>> MapListWithRolesAsync(List<ApplicationUser> users)
    {
        var DTOs = new List<GetUserDTO>();
        foreach (var user in users)
            DTOs.Add(await MapWithRoleAsync(user));
        return DTOs;
    }

    public async Task<Response<List<GetUserDTO>>> GetAllUsersAsync()
    {
        try
        {
            var result = await userRepository.GetAllAsync(new UserFilter());
            var dto = await MapListWithRolesAsync(result.Data.ToList());
            return Response<List<GetUserDTO>>.Success(dto, "Users retrieved successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting all users");
            return Response<List<GetUserDTO>>.Fail("Failed to retrieve users.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<GetUserDTO>> GetUserByIdAsync(string id)
    {
        try
        {
            var user = await userRepository.GetByIdAsync(id);
            if (user == null)
                return Response<GetUserDTO>.Fail("User not found.", HttpStatusCode.NotFound);

            var dto = await MapWithRoleAsync(user);

            var cacheKey = CacheKeys.UserProfile(id);
            await cache.SetAsync(cacheKey, dto, TimeSpan.FromHours(1));

            return Response<GetUserDTO>.Success(dto, "User retrieved successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting user with id: {Id}", id);
            return Response<GetUserDTO>.Fail("Failed to retrieve user.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<GetUserDTO>> GetCurrentUserAsync()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Response<GetUserDTO>.Fail("User not found in claims.", HttpStatusCode.Unauthorized);

            var cacheKey = CacheKeys.UserProfile(userId);
            var cached = await cache.GetAsync<GetUserDTO>(cacheKey);
            if (cached != null)
            {
                logger.LogInformation("Current user {UserId} retrieved from cache.", userId);
                return Response<GetUserDTO>.Success(cached, "Profile retrieved successfully.");
            }

            var user = await userRepository.GetByIdAsync(userId);
            if (user == null)
                return Response<GetUserDTO>.Fail("User not found.", HttpStatusCode.NotFound);

            var dto = await MapWithRoleAsync(user);
            await cache.SetAsync(cacheKey, dto, TimeSpan.FromHours(1));

            return Response<GetUserDTO>.Success(dto, "Profile retrieved successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting current user");
            return Response<GetUserDTO>.Fail("Failed to retrieve profile.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<List<GetUserDTO>>> GetUsersByRoleAsync(string role)
    {
        try
        {
            var users = await userRepository.GetByRoleAsync(role);
            var dto = await MapListWithRolesAsync(users);
            return Response<List<GetUserDTO>>.Success(dto, $"Users with role '{role}' retrieved successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting users by role: {Role}", role);
            return Response<List<GetUserDTO>>.Fail("Failed to retrieve users.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<GetUserDTO>> UpdateUserAsync(string id, UpdateUserDTO dto)
    {
        try
        {
            var user = await userRepository.GetByIdAsync(id);
            if (user == null)
                return Response<GetUserDTO>.Fail("User not found.", HttpStatusCode.NotFound);

            mapper.Map(dto, user);
            await userRepository.UpdateAsync(user);
            await cache.RemoveAsync(CacheKeys.UserProfile(id));

            var updated = await userRepository.GetByIdAsync(id);
            var resultDto = await MapWithRoleAsync(updated!);
            return Response<GetUserDTO>.Success(resultDto, "User updated successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while updating user with id: {Id}", id);
            return Response<GetUserDTO>.Fail("Failed to update user.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<GetUserDTO>> UpdateMyProfileAsync(UpdateUserDTO dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Response<GetUserDTO>.Fail("User not found in claims.", HttpStatusCode.Unauthorized);

            var user = await userRepository.GetByIdAsync(userId);
            if (user == null)
                return Response<GetUserDTO>.Fail("User not found.", HttpStatusCode.NotFound);

            mapper.Map(dto, user);
            await userRepository.UpdateAsync(user);
            await cache.RemoveAsync(CacheKeys.UserProfile(userId));

            var updated = await userRepository.GetByIdAsync(userId);
            var resultDto = await MapWithRoleAsync(updated!);
            return Response<GetUserDTO>.Success(resultDto, "Profile updated successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while updating profile");
            return Response<GetUserDTO>.Fail("Failed to update profile.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<bool>> DeleteUserAsync(string id)
    {
        try
        {
            var user = await userRepository.GetByIdAsync(id);
            if (user == null)
                return Response<bool>.Fail("User not found.", HttpStatusCode.NotFound);

            await userRepository.DeleteAsync(user);
            await cache.RemoveAsync(CacheKeys.UserProfile(id));

            return Response<bool>.Success(true, "User deleted successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while deleting user with id: {Id}", id);
            return Response<bool>.Fail("Failed to delete user.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<bool>> ChangeUserRoleAsync(string userId, string newRole)
    {
        try
        {
            var validRoles = new[] { "Admin", "Manager", "User" };
            if (!validRoles.Contains(newRole))
                return Response<bool>.Fail($"Invalid role. Valid roles: {string.Join(", ", validRoles)}", HttpStatusCode.BadRequest);

            var user = await userRepository.GetByIdAsync(userId);
            if (user == null)
                return Response<bool>.Fail("User not found.", HttpStatusCode.NotFound);

            var currentRoles = await userManager.GetRolesAsync(user);
            await userManager.RemoveFromRolesAsync(user, currentRoles);
            await userManager.AddToRoleAsync(user, newRole);
            await cache.RemoveAsync(CacheKeys.UserProfile(userId));

            return Response<bool>.Success(true, $"User role changed to '{newRole}' successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while changing role for user: {UserId}", userId);
            return Response<bool>.Fail("Failed to change user role.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<bool>> LockUserAsync(string userId)
    {
        try
        {
            var user = await userRepository.GetByIdAsync(userId);
            if (user == null)
                return Response<bool>.Fail("User not found.", HttpStatusCode.NotFound);

            if (!user.IsActive)
                return Response<bool>.Fail("User is already locked.", HttpStatusCode.BadRequest);

            user.IsActive = false;
            await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            await userRepository.UpdateAsync(user);
            await cache.RemoveAsync(CacheKeys.UserProfile(userId));

            return Response<bool>.Success(true, "User locked successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while locking user: {UserId}", userId);
            return Response<bool>.Fail("Failed to lock user.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<bool>> UnlockUserAsync(string userId)
    {
        try
        {
            var user = await userRepository.GetByIdAsync(userId);
            if (user == null)
                return Response<bool>.Fail("User not found.", HttpStatusCode.NotFound);

            if (user.IsActive)
                return Response<bool>.Fail("User is already active.", HttpStatusCode.BadRequest);

            user.IsActive = true;
            await userManager.SetLockoutEndDateAsync(user, null);
            await userRepository.UpdateAsync(user);
            await cache.RemoveAsync(CacheKeys.UserProfile(userId));

            return Response<bool>.Success(true, "User unlocked successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while unlocking user: {UserId}", userId);
            return Response<bool>.Fail("Failed to unlock user.", HttpStatusCode.InternalServerError);
        }
    }
}