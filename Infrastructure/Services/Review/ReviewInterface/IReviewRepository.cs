using Domain.ApiResponse;
using Domain.Entities;
using Domain.Filter;

namespace Infrastructure.Reviews.Interfaces;

public interface IReviewRepository
{
    Task<PagedResponse<Review>> GetAllAsync(ReviewFilter filter);
    Task<Review?> GetByIdAsync(Guid id);
    Task<List<Review>> GetByStadiumIdAsync(Guid stadiumId);
    Task<List<Review>> GetByUserIdAsync(string userId);
    Task<double> GetAverageRatingByStadiumIdAsync(Guid stadiumId);
    Task<bool> HasUserReviewedStadiumAsync(string userId, Guid stadiumId);
    Task<int> CreateAsync(Review review);
    Task<int> UpdateAsync(Review review);
    Task<int> DeleteAsync(Review review);
}