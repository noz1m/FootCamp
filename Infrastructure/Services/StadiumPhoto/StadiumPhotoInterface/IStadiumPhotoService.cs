using Domain.ApiResponse;
using Domain.DTOs.StadiumPhotoDTOs;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.StadiumPhotos.Interfaces;

public interface IStadiumPhotoService
{
    Task<Response<List<GetStadiumPhotoDTOs>>> GetAllPhotosAsync();
    Task<Response<GetStadiumPhotoDTOs>> GetPhotoByIdAsync(Guid id);
    Task<Response<List<GetStadiumPhotoDTOs>>> GetPhotosByStadiumIdAsync(Guid stadiumId);
    Task<Response<GetStadiumPhotoDTOs>> UploadPhotoAsync(Guid stadiumId, IFormFile file);
    Task<Response<List<GetStadiumPhotoDTOs>>> UploadMultiplePhotosAsync(Guid stadiumId, List<IFormFile> files);
    Task<Response<bool>> SetPrimaryPhotoAsync(Guid photoId);
    Task<Response<bool>> DeletePhotoAsync(Guid id);
    Task<Response<bool>> DeleteAllPhotosAsync(Guid stadiumId);
}