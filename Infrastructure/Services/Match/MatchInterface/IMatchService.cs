using Domain.ApiResponse;
using Domain.DTOs.MatchDTOs;

namespace Infrastructure.Matches.Interfaces;

public interface IMatchService
{
    Task<Response<List<GetMatchDTOs>>> GetAllMatchesAsync();
    Task<Response<GetMatchDTOs>> GetMatchByIdAsync(Guid id);
    Task<Response<List<GetMatchDTOs>>> GetMatchesByStadiumIdAsync(Guid stadiumId);
    Task<Response<List<GetMatchDTOs>>> GetUpcomingMatchesAsync();
    Task<Response<List<GetMatchDTOs>>> GetPastMatchesAsync();
    Task<Response<List<GetMatchDTOs>>> GetMatchesByDateAsync(DateTime date);
    Task<Response<GetMatchDTOs>> CreateMatchAsync(CreateMatchDTOs dto);
    Task<Response<GetMatchDTOs>> UpdateMatchAsync(Guid id, UpdateMatchDTOs dto);
    Task<Response<bool>> UpdateMatchStatusAsync(Guid id, string status);
    Task<Response<bool>> DeleteMatchAsync(Guid id);
}