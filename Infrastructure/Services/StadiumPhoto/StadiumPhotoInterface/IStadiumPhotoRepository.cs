using Domain.ApiResponse;
using Domain.Entities;
using Domain.Filter;

namespace Infrastructure.StadiumPhotos.Interfaces;

public interface IStadiumPhotoRepository
{
    Task<PagedResponse<StadiumPhoto>> GetAllAsync(StadiumPhotoFilter filter);
    Task<StadiumPhoto?> GetByIdAsync(Guid id);
    Task<List<StadiumPhoto>> GetByStadiumIdAsync(Guid stadiumId);
    Task<StadiumPhoto?> GetPrimaryByStadiumIdAsync(Guid stadiumId);
    Task<int> GetCountByStadiumIdAsync(Guid stadiumId);
    Task<int> CreateAsync(StadiumPhoto photo);
    Task<int> UpdateAsync(StadiumPhoto photo);
    Task<int> DeleteAsync(StadiumPhoto photo);
    Task<int> DeleteByStadiumIdAsync(Guid stadiumId);
}