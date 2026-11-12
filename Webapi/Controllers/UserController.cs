using Domain.DTOs.UserDTOs;
using Infrastructure.Users.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/users")]
public class UserController(
    IUserService userService,
    ILogger<UserController> logger) : ControllerBase
{
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        logger.LogInformation("Getting current user profile");
        var result = await userService.GetCurrentUserAsync();
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateUserDTO dto)
    {
        logger.LogInformation("Updating current user profile");
        var result = await userService.UpdateMyProfileAsync(dto);
        return StatusCode(result.StatusCode, result);
    }

    // ===== ADMIN =====

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        logger.LogInformation("Admin: getting all users");
        var result = await userService.GetAllUsersAsync();
        return StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        logger.LogInformation("Admin: getting user with id: {Id}", id);
        var result = await userService.GetUserByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("by-role/{role}")]
    public async Task<IActionResult> GetByRole(string role)
    {
        logger.LogInformation("Admin: getting users with role: {Role}", role);
        var result = await userService.GetUsersByRoleAsync(role);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDTO dto)
    {
        logger.LogInformation("Admin: updating user with id: {Id}", id);
        var result = await userService.UpdateUserAsync(id, dto);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        logger.LogInformation("Admin: deleting user with id: {Id}", id);
        var result = await userService.DeleteUserAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}/change-role")]
    public async Task<IActionResult> ChangeRole(string id, [FromQuery] string newRole)
    {
        logger.LogInformation("Admin: changing role for user {Id} to {Role}", id, newRole);
        var result = await userService.ChangeUserRoleAsync(id, newRole);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}/lock")]
    public async Task<IActionResult> Lock(string id)
    {
        logger.LogInformation("Admin: locking user with id: {Id}", id);
        var result = await userService.LockUserAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}/unlock")]
    public async Task<IActionResult> Unlock(string id)
    {
        logger.LogInformation("Admin: unlocking user with id: {Id}", id);
        var result = await userService.UnlockUserAsync(id);
        return StatusCode(result.StatusCode, result);
    }
}