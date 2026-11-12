using Domain.ApiResponse;
using Domain.Entities;
using Domain.Filter;

namespace Infrastructure.MatchRequests.Interfaces;

public interface IMatchRequestRepository
{
    Task<PagedResponse<MatchRequest>> GetAllAsync(MatchRequestFilter filter);
    Task<MatchRequest?> GetByIdAsync(Guid id);
    Task<List<MatchRequest>> GetByMatchIdAsync(Guid matchId);
    Task<List<MatchRequest>> GetByUserIdAsync(string userId);
    Task<List<MatchRequest>> GetPendingByMatchIdAsync(Guid matchId);
    Task<bool> HasUserRequestedAsync(string userId, Guid matchId);
    Task<int> CreateAsync(MatchRequest matchRequest);
    Task<int> UpdateAsync(MatchRequest matchRequest);
    Task<int> DeleteAsync(MatchRequest matchRequest);
}