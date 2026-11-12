using Domain.ApiResponse;
using Domain.Entities;
using Domain.Filter;

namespace Infrastructure.Users.Interfaces;

public interface IUserRepository
{
    Task<PagedResponse<ApplicationUser>> GetAllAsync(UserFilter filter);
    Task<ApplicationUser?> GetByIdAsync(string id);
    Task<ApplicationUser?> GetByEmailAsync(string email);
    Task<List<ApplicationUser>> GetByRoleAsync(string role);
    Task<int> UpdateAsync(ApplicationUser user);
    Task<int> DeleteAsync(ApplicationUser user);
}