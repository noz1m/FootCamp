using System.Net;
using System.Security.Claims;
using AutoMapper;
using Domain.ApiResponse;
using Domain.DTOs.StadiumDTOs;
using Domain.Entities;
using Domain.Filter;
using Infrastructure.RedisCache;
using Infrastructure.RedisCache.RedisCacheService;
using Infrastructure.Stadiums.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Stadiums.Services;

public class StadiumService(
    IStadiumRepository stadiumRepository,
    IHttpContextAccessor httpContextAccessor,
    IRedisCacheService cache,
    ILogger<StadiumService> logger,
    IMapper mapper) : IStadiumService
{
    private string? GetCurrentUserId()
    {
        return httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    public async Task<Response<List<GetStadiumDTOs>>> GetAllStadiumsAsync()
    {
        try
        {
            var cached = await cache.GetAsync<List<GetStadiumDTOs>>(CacheKeys.AllStadiums);
            if (cached != null)
            {
                logger.LogInformation("Stadiums retrieved from cache.");
                return Response<List<GetStadiumDTOs>>.Success(cached, "Stadiums retrieved successfully.");
            }

            var result = await stadiumRepository.GetAllAsync(new StadiumFilter());
            var dto = mapper.Map<List<GetStadiumDTOs>>(result.Data);

            await cache.SetAsync(CacheKeys.AllStadiums, dto, TimeSpan.FromMinutes(30));

            return Response<List<GetStadiumDTOs>>.Success(dto, "Stadiums retrieved successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting all stadiums");
            return Response<List<GetStadiumDTOs>>.Fail("Failed to retrieve stadiums.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<GetStadiumDTOs>> GetStadiumByIdAsync(Guid id)
    {
        try
        {
            var cached = await cache.GetAsync<GetStadiumDTOs>(CacheKeys.StadiumById(id));
            if (cached != null)
            {
                logger.LogInformation("Stadium {Id} retrieved from cache.", id);
                return Response<GetStadiumDTOs>.Success(cached, "Stadium retrieved successfully.");
            }

            var stadium = await stadiumRepository.GetByIdAsync(id);
            if (stadium == null)
                return Response<GetStadiumDTOs>.Fail("Stadium not found.", HttpStatusCode.NotFound);

            var dto = mapper.Map<GetStadiumDTOs>(stadium);
            await cache.SetAsync(CacheKeys.StadiumById(id), dto, TimeSpan.FromHours(1));

            return Response<GetStadiumDTOs>.Success(dto, "Stadium retrieved successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting stadium with id: {Id}", id);
            return Response<GetStadiumDTOs>.Fail("Failed to retrieve stadium.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<List<GetStadiumDTOs>>> GetMyStadiumsAsync()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Response<List<GetStadiumDTOs>>.Fail("User not found in claims.", HttpStatusCode.Unauthorized);

            var cacheKey = CacheKeys.OwnerStadiums(userId);
            var cached = await cache.GetAsync<List<GetStadiumDTOs>>(cacheKey);
            if (cached != null)
            {
                logger.LogInformation("Owner stadiums retrieved from cache for user {UserId}.", userId);
                return Response<List<GetStadiumDTOs>>.Success(cached, "Your stadiums retrieved successfully.");
            }

            var stadiums = await stadiumRepository.GetByOwnerIdAsync(userId);
            var dto = mapper.Map<List<GetStadiumDTOs>>(stadiums);

            await cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(30));

            return Response<List<GetStadiumDTOs>>.Success(dto, "Your stadiums retrieved successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting owner stadiums");
            return Response<List<GetStadiumDTOs>>.Fail("Failed to retrieve stadiums.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<List<GetStadiumDTOs>>> SearchStadiumsAsync(string searchTerm)
    {
        try
        {
            // Поиск не кешируем — результаты динамические
            var filter = new StadiumFilter { Name = searchTerm };
            var result = await stadiumRepository.GetAllAsync(filter);
            var dto = mapper.Map<List<GetStadiumDTOs>>(result.Data);
            return Response<List<GetStadiumDTOs>>.Success(dto, "Search completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while searching stadiums: {SearchTerm}", searchTerm);
            return Response<List<GetStadiumDTOs>>.Fail("Failed to search stadiums.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<List<GetStadiumDTOs>>> GetAvailableStadiumsAsync(DateTime startTime, DateTime endTime)
    {
        try
        {
            // Доступность не кешируем — меняется в реальном времени
            var stadiums = await stadiumRepository.GetAvailableAsync(startTime, endTime);
            var dto = mapper.Map<List<GetStadiumDTOs>>(stadiums);
            return Response<List<GetStadiumDTOs>>.Success(dto, "Available stadiums retrieved successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting available stadiums");
            return Response<List<GetStadiumDTOs>>.Fail("Failed to retrieve available stadiums.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<List<GetStadiumDTOs>>> GetStadiumsByLocationAsync(string location)
    {
        try
        {
            var filter = new StadiumFilter { Location = location };
            var result = await stadiumRepository.GetAllAsync(filter);
            var dto = mapper.Map<List<GetStadiumDTOs>>(result.Data);
            return Response<List<GetStadiumDTOs>>.Success(dto, "Stadiums by location retrieved successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting stadiums by location: {Location}", location);
            return Response<List<GetStadiumDTOs>>.Fail("Failed to retrieve stadiums.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<List<GetStadiumDTOs>>> GetStadiumsByPriceRangeAsync(decimal minPrice, decimal maxPrice)
    {
        try
        {
            if (minPrice < 0 || maxPrice < minPrice)
                return Response<List<GetStadiumDTOs>>.Fail("Invalid price range.", HttpStatusCode.BadRequest);

            var filter = new StadiumFilter { MinPrice = minPrice, MaxPrice = maxPrice };
            var result = await stadiumRepository.GetAllAsync(filter);
            var dto = mapper.Map<List<GetStadiumDTOs>>(result.Data);
            return Response<List<GetStadiumDTOs>>.Success(dto, "Stadiums by price range retrieved successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting stadiums by price range");
            return Response<List<GetStadiumDTOs>>.Fail("Failed to retrieve stadiums.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<GetStadiumDTOs>> CreateStadiumAsync(CreateStadiumDTOs dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Response<GetStadiumDTOs>.Fail("User not found in claims.", HttpStatusCode.Unauthorized);

            var stadium = mapper.Map<Stadium>(dto);
            stadium.Id = Guid.NewGuid();
            stadium.OwnerId = userId;
            stadium.IsActive = true;
            stadium.CreatedAt = DateTime.UtcNow;

            await stadiumRepository.CreateAsync(stadium);

            await cache.RemoveAsync(CacheKeys.AllStadiums);
            await cache.RemoveAsync(CacheKeys.OwnerStadiums(userId));

            var result = await stadiumRepository.GetByIdAsync(stadium.Id);
            var resultDto = mapper.Map<GetStadiumDTOs>(result);
            return Response<GetStadiumDTOs>.Success(resultDto, "Stadium created successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while creating stadium");
            return Response<GetStadiumDTOs>.Fail("Failed to create stadium.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<GetStadiumDTOs>> UpdateStadiumAsync(Guid id, UpdateStadiumDTOs dto)
    {
        try
        {
            var stadium = await stadiumRepository.GetByIdAsync(id);
            if (stadium == null)
                return Response<GetStadiumDTOs>.Fail("Stadium not found.", HttpStatusCode.NotFound);

            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Response<GetStadiumDTOs>.Fail("User not found in claims.", HttpStatusCode.Unauthorized);

            if (stadium.OwnerId != userId)
                return Response<GetStadiumDTOs>.Fail("You are not authorized to update this stadium.", HttpStatusCode.Forbidden);

            mapper.Map(dto, stadium);
            await stadiumRepository.UpdateAsync(stadium);

            await cache.RemoveAsync(CacheKeys.StadiumById(id));
            await cache.RemoveAsync(CacheKeys.AllStadiums);
            await cache.RemoveAsync(CacheKeys.OwnerStadiums(userId));

            var result = await stadiumRepository.GetByIdAsync(id);
            var resultDto = mapper.Map<GetStadiumDTOs>(result);
            return Response<GetStadiumDTOs>.Success(resultDto, "Stadium updated successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while updating stadium with id: {Id}", id);
            return Response<GetStadiumDTOs>.Fail("Failed to update stadium.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<bool>> DeleteStadiumAsync(Guid id)
    {
        try
        {
            var stadium = await stadiumRepository.GetByIdAsync(id);
            if (stadium == null)
                return Response<bool>.Fail("Stadium not found.", HttpStatusCode.NotFound);

            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Response<bool>.Fail("User not found in claims.", HttpStatusCode.Unauthorized);

            var isAdmin = httpContextAccessor.HttpContext?.User?.IsInRole("Admin") ?? false;
            if (stadium.OwnerId != userId && !isAdmin)
                return Response<bool>.Fail("You are not authorized to delete this stadium.", HttpStatusCode.Forbidden);

            await stadiumRepository.DeleteAsync(stadium);

            await cache.RemoveAsync(CacheKeys.StadiumById(id));
            await cache.RemoveAsync(CacheKeys.AllStadiums);
            await cache.RemoveAsync(CacheKeys.OwnerStadiums(stadium.OwnerId));
            await cache.RemoveAsync(CacheKeys.StadiumReviews(id));

            return Response<bool>.Success(true, "Stadium deleted successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while deleting stadium with id: {Id}", id);
            return Response<bool>.Fail("Failed to delete stadium.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<bool>> ActivateStadiumAsync(Guid id)
    {
        try
        {
            var stadium = await stadiumRepository.GetByIdAsync(id);
            if (stadium == null)
                return Response<bool>.Fail("Stadium not found.", HttpStatusCode.NotFound);

            stadium.IsActive = true;
            await stadiumRepository.UpdateAsync(stadium);

            await cache.RemoveAsync(CacheKeys.StadiumById(id));
            await cache.RemoveAsync(CacheKeys.AllStadiums);

            return Response<bool>.Success(true, "Stadium activated successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while activating stadium with id: {Id}", id);
            return Response<bool>.Fail("Failed to activate stadium.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<bool>> DeactivateStadiumAsync(Guid id)
    {
        try
        {
            var stadium = await stadiumRepository.GetByIdAsync(id);
            if (stadium == null)
                return Response<bool>.Fail("Stadium not found.", HttpStatusCode.NotFound);

            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Response<bool>.Fail("User not found in claims.", HttpStatusCode.Unauthorized);

            if (stadium.OwnerId != userId)
                return Response<bool>.Fail("You are not authorized to deactivate this stadium.", HttpStatusCode.Forbidden);

            stadium.IsActive = false;
            await stadiumRepository.UpdateAsync(stadium);

            await cache.RemoveAsync(CacheKeys.StadiumById(id));
            await cache.RemoveAsync(CacheKeys.AllStadiums);
            await cache.RemoveAsync(CacheKeys.OwnerStadiums(userId));

            return Response<bool>.Success(true, "Stadium deactivated successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while deactivating stadium with id: {Id}", id);
            return Response<bool>.Fail("Failed to deactivate stadium.", HttpStatusCode.InternalServerError);
        }
    }
}