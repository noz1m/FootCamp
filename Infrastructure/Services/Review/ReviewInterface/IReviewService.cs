using Domain.ApiResponse;
using Domain.DTOs.ReviewDTOs;

namespace Infrastructure.Reviews.Interfaces;

public interface IReviewService
{
    Task<Response<List<GetReviewDTOs>>> GetAllReviewsAsync();
    Task<Response<GetReviewDTOs>> GetReviewByIdAsync(Guid id);
    Task<Response<List<GetReviewDTOs>>> GetReviewsByStadiumIdAsync(Guid stadiumId);
    Task<Response<List<GetReviewDTOs>>> GetMyReviewsAsync();
    Task<Response<double>> GetAverageRatingAsync(Guid stadiumId);
    Task<Response<GetReviewDTOs>> CreateReviewAsync(CreateReviewDTOs dto);
    Task<Response<GetReviewDTOs>> UpdateReviewAsync(Guid id, UpdateReviewDTOs dto);
    Task<Response<bool>> DeleteReviewAsync(Guid id);
}