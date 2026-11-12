using Domain.ApiResponse;
using Domain.Entities;
using Domain.Filter;
using Domain.Pagination;
using Infrastructure.Data;
using Infrastructure.Stadiums.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Stadiums.Repositories;

public class StadiumRepository(
    DataContext context,
    ILogger<StadiumRepository> logger,
    Pagination<Stadium> pagination) : IStadiumRepository
{
    public async Task<PagedResponse<Stadium>> GetAllAsync(StadiumFilter filter)
    {
        try
        {
            var query = context.Stadiums
                .Include(s => s.Owner)
                .Include(s => s.Photos)
                .Include(s => s.Reviews)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filter.Name))
                query = query.Where(s => s.Name.ToLower().Contains(filter.Name.ToLower()));

            if (!string.IsNullOrEmpty(filter.Location))
                query = query.Where(s => s.City.ToLower().Contains(filter.Location.ToLower()) ||
                                         s.Address.ToLower().Contains(filter.Location.ToLower()));

            if (filter.MinCapacity != null)
                query = query.Where(s => s.Capacity >= filter.MinCapacity);

            if (filter.MaxCapacity != null)
                query = query.Where(s => s.Capacity <= filter.MaxCapacity);

            if (filter.MinPrice != null)
                query = query.Where(s => s.PricePerHour >= filter.MinPrice);

            if (filter.MaxPrice != null)
                query = query.Where(s => s.PricePerHour <= filter.MaxPrice);

            if (filter.IsActive != null)
                query = query.Where(s => s.IsActive == filter.IsActive);

            if (!string.IsNullOrEmpty(filter.OwnerEmail))
                query = query.Where(s => s.Owner.Email != null &&
                                         s.Owner.Email.ToLower().Contains(filter.OwnerEmail.ToLower()));

            if (filter.CreatedFrom != null)
                query = query.Where(s => s.CreatedAt >= filter.CreatedFrom);

            if (filter.CreatedTo != null)
                query = query.Where(s => s.CreatedAt <= filter.CreatedTo);

            // Фильтр по радиусу (если указаны координаты)
            if (filter.Latitude != null && filter.Longitude != null && filter.RadiusKm != null)
            {
                var lat = filter.Latitude.Value;
                var lng = filter.Longitude.Value;
                var radius = filter.RadiusKm.Value;

                query = query.Where(s =>
                    Math.Sqrt(
                        Math.Pow((s.Latitude - lat) * 111.32, 2) +
                        Math.Pow((s.Longitude - lng) * 111.32 * Math.Cos(lat * Math.PI / 180), 2)
                    ) <= radius);
            }

            return await pagination.GetPagedAsync(query, filter.PageNumber, filter.PageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting all stadiums");
            throw;
        }
    }

    public async Task<Stadium?> GetByIdAsync(Guid id)
    {
        try
        {
            return await context.Stadiums
                .Include(s => s.Owner)
                .Include(s => s.Photos)
                .Include(s => s.Reviews)
                .Include(s => s.Bookings)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting stadium with id: {Id}", id);
            throw;
        }
    }

    public async Task<List<Stadium>> GetByOwnerIdAsync(string ownerId)
    {
        try
        {
            return await context.Stadiums
                .Include(s => s.Photos)
                .Include(s => s.Reviews)
                .Where(s => s.OwnerId == ownerId)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting stadiums by ownerId: {OwnerId}", ownerId);
            throw;
        }
    }

    public async Task<List<Stadium>> GetAvailableAsync(DateTime startTime, DateTime endTime)
    {
        try
        {
            var bookedStadiumIds = await context.Bookings
                .Where(b =>
                    b.Status != "cancelled" &&
                    b.StartTime < endTime &&
                    b.EndTime > startTime)
                .Select(b => b.StadiumId)
                .Distinct()
                .ToListAsync();

            return await context.Stadiums
                .Include(s => s.Photos)
                .Include(s => s.Reviews)
                .Where(s => s.IsActive && !bookedStadiumIds.Contains(s.Id))
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting available stadiums");
            throw;
        }
    }

    public async Task<int> CreateAsync(Stadium stadium)
    {
        try
        {
            await context.Stadiums.AddAsync(stadium);
            return await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while creating stadium");
            throw;
        }
    }

    public async Task<int> UpdateAsync(Stadium stadium)
    {
        try
        {
            stadium.UpdatedAt = DateTime.UtcNow;
            context.Stadiums.Update(stadium);
            return await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while updating stadium with id: {Id}", stadium.Id);
            throw;
        }
    }

    public async Task<int> DeleteAsync(Stadium stadium)
    {
        try
        {
            context.Stadiums.Remove(stadium);
            return await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while deleting stadium with id: {Id}", stadium.Id);
            throw;
        }
    }
}