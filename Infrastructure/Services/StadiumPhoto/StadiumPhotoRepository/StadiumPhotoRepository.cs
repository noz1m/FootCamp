using Domain.ApiResponse;
using Domain.Entities;
using Domain.Filter;
using Domain.Pagination;
using Infrastructure.Data;
using Infrastructure.StadiumPhotos.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.StadiumPhotos.Repositories;

public class StadiumPhotoRepository(
    DataContext context,
    ILogger<StadiumPhotoRepository> logger,
    Pagination<StadiumPhoto> pagination) : IStadiumPhotoRepository
{
    public async Task<PagedResponse<StadiumPhoto>> GetAllAsync(StadiumPhotoFilter filter)
    {
        try
        {
            var query = context.StadiumPhotos
                .Include(sp => sp.Stadium)
                .AsQueryable();

            if (filter.StadiumId != null)
                query = query.Where(sp => sp.StadiumId == filter.StadiumId);

            if (filter.IsPrimary != null)
                query = query.Where(sp => sp.IsPrimary == filter.IsPrimary);

            return await pagination.GetPagedAsync(query, filter.PageNumber, filter.PageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting all stadium photos");
            throw;
        }
    }

    public async Task<StadiumPhoto?> GetByIdAsync(Guid id)
    {
        try
        {
            return await context.StadiumPhotos
                .Include(sp => sp.Stadium)
                .AsNoTracking()
                .FirstOrDefaultAsync(sp => sp.Id == id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting photo with id: {Id}", id);
            throw;
        }
    }

    public async Task<List<StadiumPhoto>> GetByStadiumIdAsync(Guid stadiumId)
    {
        try
        {
            return await context.StadiumPhotos
                .Where(sp => sp.StadiumId == stadiumId)
                .OrderByDescending(sp => sp.IsPrimary)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting photos by stadiumId: {StadiumId}", stadiumId);
            throw;
        }
    }

    public async Task<StadiumPhoto?> GetPrimaryByStadiumIdAsync(Guid stadiumId)
    {
        try
        {
            return await context.StadiumPhotos
                .AsNoTracking()
                .FirstOrDefaultAsync(sp => sp.StadiumId == stadiumId && sp.IsPrimary);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting primary photo for stadiumId: {StadiumId}", stadiumId);
            throw;
        }
    }

    public async Task<int> GetCountByStadiumIdAsync(Guid stadiumId)
    {
        try
        {
            return await context.StadiumPhotos
                .CountAsync(sp => sp.StadiumId == stadiumId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting photo count for stadiumId: {StadiumId}", stadiumId);
            throw;
        }
    }

    public async Task<int> CreateAsync(StadiumPhoto photo)
    {
        try
        {
            await context.StadiumPhotos.AddAsync(photo);
            return await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while creating stadium photo");
            throw;
        }
    }

    public async Task<int> UpdateAsync(StadiumPhoto photo)
    {
        try
        {
            context.StadiumPhotos.Update(photo);
            return await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while updating photo with id: {Id}", photo.Id);
            throw;
        }
    }

    public async Task<int> DeleteAsync(StadiumPhoto photo)
    {
        try
        {
            context.StadiumPhotos.Remove(photo);
            return await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while deleting photo with id: {Id}", photo.Id);
            throw;
        }
    }

    public async Task<int> DeleteByStadiumIdAsync(Guid stadiumId)
    {
        try
        {
            var photos = await context.StadiumPhotos
                .Where(sp => sp.StadiumId == stadiumId)
                .ToListAsync();

            context.StadiumPhotos.RemoveRange(photos);
            return await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while deleting photos for stadiumId: {StadiumId}", stadiumId);
            throw;
        }
    }
}