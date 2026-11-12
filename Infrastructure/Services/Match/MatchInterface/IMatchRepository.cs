using Domain.ApiResponse;
using Domain.Entities;
using Domain.Filter;

namespace Infrastructure.Matches.Interfaces;

public interface IMatchRepository
{
    Task<PagedResponse<Match>> GetAllAsync(MatchFilter filter);
    Task<Match?> GetByIdAsync(Guid id);
    Task<List<Match>> GetByStadiumIdAsync(Guid stadiumId);
    Task<List<Match>> GetByCreatorIdAsync(string creatorId);
    Task<List<Match>> GetUpcomingAsync();
    Task<List<Match>> GetPastAsync();
    Task<List<Match>> GetByDateAsync(DateTime date);
    Task<List<Match>> GetByStatusAsync(string status);
    Task<int> CreateAsync(Match match);
    Task<int> UpdateAsync(Match match);
    Task<int> DeleteAsync(Match match);
}