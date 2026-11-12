using Domain.ApiResponse;
using Domain.DTOs.StadiumDTOs;

namespace Infrastructure.Stadiums.Interfaces;

public interface IStadiumService
{
    Task<Response<List<GetStadiumDTOs>>> GetAllStadiumsAsync();
    Task<Response<GetStadiumDTOs>> GetStadiumByIdAsync(Guid id);
    Task<Response<List<GetStadiumDTOs>>> GetMyStadiumsAsync();
    Task<Response<List<GetStadiumDTOs>>> SearchStadiumsAsync(string searchTerm);
    Task<Response<List<GetStadiumDTOs>>> GetAvailableStadiumsAsync(DateTime startTime, DateTime endTime);
    Task<Response<List<GetStadiumDTOs>>> GetStadiumsByLocationAsync(string location);
    Task<Response<List<GetStadiumDTOs>>> GetStadiumsByPriceRangeAsync(decimal minPrice, decimal maxPrice);
    Task<Response<GetStadiumDTOs>> CreateStadiumAsync(CreateStadiumDTOs dto);
    Task<Response<GetStadiumDTOs>> UpdateStadiumAsync(Guid id, UpdateStadiumDTOs dto);
    Task<Response<bool>> DeleteStadiumAsync(Guid id);
    Task<Response<bool>> ActivateStadiumAsync(Guid id);
    Task<Response<bool>> DeactivateStadiumAsync(Guid id);
}