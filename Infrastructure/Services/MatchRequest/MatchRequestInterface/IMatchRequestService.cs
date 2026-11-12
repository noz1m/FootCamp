using Domain.ApiResponse;
using Domain.DTOs.MatchRequestDTOs;


namespace Infrastructure.MatchRequests.Interfaces;

public interface IMatchRequestService
{
    Task<Response<List<GetMatchRequestDTOs>>> GetAllMatchRequestsAsync();
    Task<Response<GetMatchRequestDTOs>> GetMatchRequestByIdAsync(Guid id);
    Task<Response<List<GetMatchRequestDTOs>>> GetMyMatchRequestsAsync();
    Task<Response<List<GetMatchRequestDTOs>>> GetMatchRequestsByMatchIdAsync(Guid matchId);
    Task<Response<List<GetMatchRequestDTOs>>> GetPendingRequestsAsync(Guid matchId);
    Task<Response<GetMatchRequestDTOs>> CreateMatchRequestAsync(CreateMatchRequestDTOs dto);
    Task<Response<GetMatchRequestDTOs>> UpdateMatchRequestAsync(Guid id, UpdateMatchRequestDTOs dto);
    Task<Response<bool>> ApproveMatchRequestAsync(Guid id);
    Task<Response<bool>> RejectMatchRequestAsync(Guid id);
    Task<Response<bool>> DeleteMatchRequestAsync(Guid id);
}