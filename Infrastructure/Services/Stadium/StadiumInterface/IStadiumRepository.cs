using Domain.ApiResponse;
using Domain.Entities;
using Domain.Filter;


namespace Infrastructure.Stadiums.Interfaces;

public interface IStadiumRepository
{
    Task<PagedResponse<Stadium>> GetAllAsync(StadiumFilter filter);
    Task<Stadium?> GetByIdAsync(Guid id);
    Task<List<Stadium>> GetByOwnerIdAsync(string ownerId);
    Task<List<Stadium>> GetAvailableAsync(DateTime startTime, DateTime endTime);
    Task<int> CreateAsync(Stadium stadium);
    Task<int> UpdateAsync(Stadium stadium);
    Task<int> DeleteAsync(Stadium stadium);
}