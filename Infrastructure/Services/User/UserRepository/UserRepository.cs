using Domain.ApiResponse;
using Domain.Entities;
using Domain.Pagination;
using Domain.Filter;
using Infrastructure.Data;
using Infrastructure.Users.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Users.Repositories;

public class UserRepository(
    DataContext context,
    UserManager<ApplicationUser> userManager,
    ILogger<UserRepository> logger,
    Pagination<ApplicationUser> pagination) : IUserRepository
{
    public async Task<PagedResponse<ApplicationUser>> GetAllAsync(UserFilter filter)
    {
        try
        {
            var query = userManager.Users
                .Include(u => u.OwnedStadiums)
                .Include(u => u.Bookings)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filter.Email))
                query = query.Where(u => u.Email != null &&
                                         u.Email.ToLower().Contains(filter.Email.ToLower()));

            if (!string.IsNullOrEmpty(filter.FullName))
                query = query.Where(u => u.FullName.ToLower().Contains(filter.FullName.ToLower()));

            if (filter.IsActive != null)
                query = query.Where(u => u.IsActive == filter.IsActive);

            if (filter.EmailConfirmed != null)
                query = query.Where(u => u.EmailConfirmed == filter.EmailConfirmed);

            if (filter.CreatedFrom != null)
                query = query.Where(u => u.CreatedAt >= filter.CreatedFrom);

            if (filter.CreatedTo != null)
                query = query.Where(u => u.CreatedAt <= filter.CreatedTo);

            // Filter by role if specified
            if (!string.IsNullOrEmpty(filter.Role))
            {
                var usersInRole = await userManager.GetUsersInRoleAsync(filter.Role);
                var userIds = usersInRole.Select(u => u.Id).ToList();
                query = query.Where(u => userIds.Contains(u.Id));
            }

            return await pagination.GetPagedAsync(query, filter.PageNumber, filter.PageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting all users");
            throw;
        }
    }

    public async Task<ApplicationUser?> GetByIdAsync(string id)
    {
        try
        {
            return await userManager.Users
                .Include(u => u.OwnedStadiums)
                .Include(u => u.Bookings)
                .Include(u => u.Reviews)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting user with id: {Id}", id);
            throw;
        }
    }

    public async Task<ApplicationUser?> GetByEmailAsync(string email)
    {
        try
        {
            return await userManager.Users
                .Include(u => u.OwnedStadiums)
                .Include(u => u.Bookings)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email != null &&
                                          u.Email.ToLower() == email.ToLower());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting user by email: {Email}", email);
            throw;
        }
    }

    public async Task<List<ApplicationUser>> GetByRoleAsync(string role)
    {
        try
        {
            var users = await userManager.GetUsersInRoleAsync(role);
            return users.ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting users by role: {Role}", role);
            throw;
        }
    }

    public async Task<int> UpdateAsync(ApplicationUser user)
    {
        try
        {
            user.UpdatedAt = DateTime.UtcNow;
            var result = await userManager.UpdateAsync(user);

            return result.Succeeded ? 1 : 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while updating user with id: {Id}", user.Id);
            throw;
        }
    }

    public async Task<int> DeleteAsync(ApplicationUser user)
    {
        try
        {
            var result = await userManager.DeleteAsync(user);
            return result.Succeeded ? 1 : 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while deleting user with id: {Id}", user.Id);
            throw;
        }
    }
}