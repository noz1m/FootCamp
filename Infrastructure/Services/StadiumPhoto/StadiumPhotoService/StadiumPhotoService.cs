using System.Net;
using System.Security.Claims;
using AutoMapper;
using Domain.ApiResponse;
using Domain.DTOs.StadiumPhotoDTOs;
using Domain.Entities;
using Domain.Filter;
using Infrastructure.StadiumPhotos.Interfaces;
using Infrastructure.Stadiums.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Infrastructure.StadiumPhotos.Services;

public class StadiumPhotoService(
    IStadiumPhotoRepository stadiumPhotoRepository,
    IStadiumRepository stadiumRepository,
    IHttpContextAccessor httpContextAccessor,
    ILogger<StadiumPhotoService> logger,
    IMapper mapper) : IStadiumPhotoService
{
    private string? GetCurrentUserId()
    {
        return httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    public async Task<Response<List<GetStadiumPhotoDTOs>>> GetAllPhotosAsync()
    {
        try
        {
            // Repository.GetAllAsync принимает StadiumPhotoFilter
            var result = await stadiumPhotoRepository.GetAllAsync(new StadiumPhotoFilter());
            var dto = mapper.Map<List<GetStadiumPhotoDTOs>>(result.Data);
            return Response<List<GetStadiumPhotoDTOs>>.Success(dto, "Photos retrieved successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting all photos");
            return Response<List<GetStadiumPhotoDTOs>>.Fail("Failed to retrieve photos.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<GetStadiumPhotoDTOs>> GetPhotoByIdAsync(Guid id)
    {
        try
        {
            var photo = await stadiumPhotoRepository.GetByIdAsync(id);
            if (photo == null)
                return Response<GetStadiumPhotoDTOs>.Fail("Photo not found.", HttpStatusCode.NotFound);

            var dto = mapper.Map<GetStadiumPhotoDTOs>(photo);
            return Response<GetStadiumPhotoDTOs>.Success(dto, "Photo retrieved successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting photo with id: {Id}", id);
            return Response<GetStadiumPhotoDTOs>.Fail("Failed to retrieve photo.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<List<GetStadiumPhotoDTOs>>> GetPhotosByStadiumIdAsync(Guid stadiumId)
    {
        try
        {
            // Repository имеет GetByStadiumIdAsync — используем его напрямую
            var photos = await stadiumPhotoRepository.GetByStadiumIdAsync(stadiumId);
            var dto = mapper.Map<List<GetStadiumPhotoDTOs>>(photos);
            return Response<List<GetStadiumPhotoDTOs>>.Success(dto, "Stadium photos retrieved successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting photos for stadium: {StadiumId}", stadiumId);
            return Response<List<GetStadiumPhotoDTOs>>.Fail("Failed to retrieve photos.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<GetStadiumPhotoDTOs>> UploadPhotoAsync(Guid stadiumId, IFormFile file)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Response<GetStadiumPhotoDTOs>.Fail("User not found in claims.", HttpStatusCode.Unauthorized);

            var stadium = await stadiumRepository.GetByIdAsync(stadiumId);
            if (stadium == null)
                return Response<GetStadiumPhotoDTOs>.Fail("Stadium not found.", HttpStatusCode.NotFound);

            if (stadium.OwnerId != userId)
                return Response<GetStadiumPhotoDTOs>.Fail("You are not authorized to upload photos to this stadium.", HttpStatusCode.Forbidden);

            // Repository.GetCountByStadiumIdAsync — проверяем лимит 10 фото
            var count = await stadiumPhotoRepository.GetCountByStadiumIdAsync(stadiumId);
            if (count >= 10)
                return Response<GetStadiumPhotoDTOs>.Fail("Maximum 10 photos allowed per stadium.", HttpStatusCode.BadRequest);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var uploadPath = Path.Combine("wwwroot", "uploads", "stadiums");
            Directory.CreateDirectory(uploadPath);

            var filePath = Path.Combine(uploadPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
                await file.CopyToAsync(stream);

            var photo = new StadiumPhoto
            {
                Id = Guid.NewGuid(),
                StadiumId = stadiumId,
                PhotoUrl = $"/uploads/stadiums/{fileName}",
                IsPrimary = count == 0, // первое фото — главное
                CreatedAt = DateTime.UtcNow
            };

            await stadiumPhotoRepository.CreateAsync(photo);

            var result = await stadiumPhotoRepository.GetByIdAsync(photo.Id);
            var resultDto = mapper.Map<GetStadiumPhotoDTOs>(result);
            return Response<GetStadiumPhotoDTOs>.Success(resultDto, "Photo uploaded successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while uploading photo for stadium: {StadiumId}", stadiumId);
            return Response<GetStadiumPhotoDTOs>.Fail("Failed to upload photo.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<List<GetStadiumPhotoDTOs>>> UploadMultiplePhotosAsync(Guid stadiumId, List<IFormFile> files)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Response<List<GetStadiumPhotoDTOs>>.Fail("User not found in claims.", HttpStatusCode.Unauthorized);

            var stadium = await stadiumRepository.GetByIdAsync(stadiumId);
            if (stadium == null)
                return Response<List<GetStadiumPhotoDTOs>>.Fail("Stadium not found.", HttpStatusCode.NotFound);

            if (stadium.OwnerId != userId)
                return Response<List<GetStadiumPhotoDTOs>>.Fail("You are not authorized to upload photos to this stadium.", HttpStatusCode.Forbidden);

            var count = await stadiumPhotoRepository.GetCountByStadiumIdAsync(stadiumId);
            if (count + files.Count > 10)
                return Response<List<GetStadiumPhotoDTOs>>.Fail(
                    $"Cannot upload {files.Count} photos. Maximum 10 allowed, currently {count}.",
                    HttpStatusCode.BadRequest);

            var uploaded = new List<GetStadiumPhotoDTOs>();
            foreach (var file in files)
            {
                var result = await UploadPhotoAsync(stadiumId, file);
                if (result.Data != null)
                    uploaded.Add(result.Data);
            }

            return Response<List<GetStadiumPhotoDTOs>>.Success(uploaded, $"{uploaded.Count} photos uploaded successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while uploading multiple photos for stadium: {StadiumId}", stadiumId);
            return Response<List<GetStadiumPhotoDTOs>>.Fail("Failed to upload photos.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<bool>> SetPrimaryPhotoAsync(Guid photoId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Response<bool>.Fail("User not found in claims.", HttpStatusCode.Unauthorized);

            var photo = await stadiumPhotoRepository.GetByIdAsync(photoId);
            if (photo == null)
                return Response<bool>.Fail("Photo not found.", HttpStatusCode.NotFound);

            var stadium = await stadiumRepository.GetByIdAsync(photo.StadiumId);
            if (stadium == null)
                return Response<bool>.Fail("Stadium not found.", HttpStatusCode.NotFound);

            if (stadium.OwnerId != userId)
                return Response<bool>.Fail("You are not authorized to manage this stadium's photos.", HttpStatusCode.Forbidden);

            // Снимаем IsPrimary со старого, ставим на новое
            var currentPrimary = await stadiumPhotoRepository.GetPrimaryByStadiumIdAsync(photo.StadiumId);
            if (currentPrimary != null && currentPrimary.Id != photoId)
            {
                currentPrimary.IsPrimary = false;
                await stadiumPhotoRepository.UpdateAsync(currentPrimary);
            }

            photo.IsPrimary = true;
            await stadiumPhotoRepository.UpdateAsync(photo);

            return Response<bool>.Success(true, "Primary photo updated successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while setting primary photo: {PhotoId}", photoId);
            return Response<bool>.Fail("Failed to set primary photo.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<bool>> DeletePhotoAsync(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Response<bool>.Fail("User not found in claims.", HttpStatusCode.Unauthorized);

            var photo = await stadiumPhotoRepository.GetByIdAsync(id);
            if (photo == null)
                return Response<bool>.Fail("Photo not found.", HttpStatusCode.NotFound);

            var stadium = await stadiumRepository.GetByIdAsync(photo.StadiumId);
            if (stadium == null)
                return Response<bool>.Fail("Stadium not found.", HttpStatusCode.NotFound);

            if (stadium.OwnerId != userId)
                return Response<bool>.Fail("You are not authorized to delete this photo.", HttpStatusCode.Forbidden);

            await stadiumPhotoRepository.DeleteAsync(photo);

            // Если удалили главное — назначаем первое из оставшихся
            if (photo.IsPrimary)
            {
                var remaining = await stadiumPhotoRepository.GetByStadiumIdAsync(photo.StadiumId);
                if (remaining.Count > 0)
                {
                    remaining[0].IsPrimary = true;
                    await stadiumPhotoRepository.UpdateAsync(remaining[0]);
                }
            }

            return Response<bool>.Success(true, "Photo deleted successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while deleting photo: {Id}", id);
            return Response<bool>.Fail("Failed to delete photo.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<bool>> DeleteAllPhotosAsync(Guid stadiumId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Response<bool>.Fail("User not found in claims.", HttpStatusCode.Unauthorized);

            var stadium = await stadiumRepository.GetByIdAsync(stadiumId);
            if (stadium == null)
                return Response<bool>.Fail("Stadium not found.", HttpStatusCode.NotFound);

            if (stadium.OwnerId != userId)
                return Response<bool>.Fail("You are not authorized to delete photos from this stadium.", HttpStatusCode.Forbidden);

            // Repository имеет DeleteByStadiumIdAsync
            await stadiumPhotoRepository.DeleteByStadiumIdAsync(stadiumId);
            return Response<bool>.Success(true, "All photos deleted successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while deleting all photos for stadium: {StadiumId}", stadiumId);
            return Response<bool>.Fail("Failed to delete photos.", HttpStatusCode.InternalServerError);
        }
    }
}