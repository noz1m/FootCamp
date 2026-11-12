using Domain.ApiResponse;
using Domain.Entities;
using Domain.Pagination;
using Domain.Filter;
using Infrastructure.Data;
using Infrastructure.Reviews.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Reviews.Repositories;

public class ReviewRepository(
    DataContext context,
    ILogger<ReviewRepository> logger,
    Pagination<Review> pagination) : IReviewRepository
{
    public async Task<PagedResponse<Review>> GetAllAsync(ReviewFilter filter)
    {
        try
        {
            var query = context.Reviews
                .Include(r => r.Stadium)
                .Include(r => r.User)
                .AsQueryable();

            if (filter.StadiumId != null)
                query = query.Where(r => r.StadiumId == filter.StadiumId);

            if (filter.UserId != null)
                query = query.Where(r => r.UserId == filter.UserId);

            if (filter.MinRating != null)
                query = query.Where(r => r.Rating >= filter.MinRating);

            if (filter.MaxRating != null)
                query = query.Where(r => r.Rating <= filter.MaxRating);

            if (filter.From != null)
                query = query.Where(r => r.CreatedAt >= filter.From);

            if (filter.To != null)
                query = query.Where(r => r.CreatedAt <= filter.To);

            return await pagination.GetPagedAsync(query, filter.PageNumber, filter.PageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting all reviews");
            throw;
        }
    }

    public async Task<Review?> GetByIdAsync(Guid id)
    {
        try
        {
            return await context.Reviews
                .Include(r => r.Stadium)
                .Include(r => r.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting review with id: {Id}", id);
            throw;
        }
    }

    public async Task<List<Review>> GetByStadiumIdAsync(Guid stadiumId)
    {
        try
        {
            return await context.Reviews
                .Include(r => r.User)
                .Where(r => r.StadiumId == stadiumId)
                .OrderByDescending(r => r.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting reviews by stadiumId: {StadiumId}", stadiumId);
            throw;
        }
    }

    public async Task<List<Review>> GetByUserIdAsync(string userId)
    {
        try
        {
            return await context.Reviews
                .Include(r => r.Stadium)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting reviews by userId: {UserId}", userId);
            throw;
        }
    }

    public async Task<double> GetAverageRatingByStadiumIdAsync(Guid stadiumId)
    {
        try
        {
            var reviews = await context.Reviews
                .Where(r => r.StadiumId == stadiumId)
                .ToListAsync();

            return reviews.Any() ? reviews.Average(r => r.Rating) : 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting average rating for stadiumId: {StadiumId}", stadiumId);
            throw;
        }
    }

    public async Task<bool> HasUserReviewedStadiumAsync(string userId, Guid stadiumId)
    {
        try
        {
            return await context.Reviews
                .AnyAsync(r => r.UserId == userId && r.StadiumId == stadiumId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while checking user review for stadiumId: {StadiumId}", stadiumId);
            throw;
        }
    }

    public async Task<int> CreateAsync(Review review)
    {
        try
        {
            await context.Reviews.AddAsync(review);
            return await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while creating review");
            throw;
        }
    }

    public async Task<int> UpdateAsync(Review review)
    {
        try
        {
            review.UpdatedAt = DateTime.UtcNow;
            context.Reviews.Update(review);
            return await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while updating review with id: {Id}", review.Id);
            throw;
        }
    }

    public async Task<int> DeleteAsync(Review review)
    {
        try
        {
            context.Reviews.Remove(review);
            return await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while deleting review with id: {Id}", review.Id);
            throw;
        }
    }
}