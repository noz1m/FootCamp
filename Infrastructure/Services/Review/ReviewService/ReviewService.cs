using System.Net;
using System.Security.Claims;
using AutoMapper;
using Domain.ApiResponse;
using Domain.DTOs.ReviewDTOs;
using Domain.Entities;
using Domain.Filter;
using Infrastructure.RedisCache;
using Infrastructure.RedisCache.RedisCacheService;
using Infrastructure.Reviews.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Reviews.Services;

public class ReviewService(
    IReviewRepository reviewRepository,
    IHttpContextAccessor httpContextAccessor,
    IRedisCacheService cache,
    ILogger<ReviewService> logger,
    IMapper mapper) : IReviewService
{
    private string? GetCurrentUserId()
    {
        return httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    public async Task<Response<List<GetReviewDTOs>>> GetAllReviewsAsync()
    {
        try
        {
            var filter = new ReviewFilter();
            var result = await reviewRepository.GetAllAsync(filter);
            var dto = mapper.Map<List<GetReviewDTOs>>(result.Data);
            return Response<List<GetReviewDTOs>>.Success(dto, "Reviews retrieved successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting all reviews");
            return Response<List<GetReviewDTOs>>.Fail("Failed to retrieve reviews.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<GetReviewDTOs>> GetReviewByIdAsync(Guid id)
    {
        try
        {
            var review = await reviewRepository.GetByIdAsync(id);
            if (review == null)
                return Response<GetReviewDTOs>.Fail("Review not found.", HttpStatusCode.NotFound);

            var dto = mapper.Map<GetReviewDTOs>(review);
            return Response<GetReviewDTOs>.Success(dto, "Review retrieved successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting review with id: {Id}", id);
            return Response<GetReviewDTOs>.Fail("Failed to retrieve review.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<List<GetReviewDTOs>>> GetReviewsByStadiumIdAsync(Guid stadiumId)
    {
        try
        {
            var cacheKey = CacheKeys.StadiumReviews(stadiumId);
            var cached = await cache.GetAsync<List<GetReviewDTOs>>(cacheKey);
            if (cached != null)
            {
                logger.LogInformation("Reviews for stadium {StadiumId} retrieved from cache.", stadiumId);
                return Response<List<GetReviewDTOs>>.Success(cached, "Stadium reviews retrieved successfully.");
            }

            var reviews = await reviewRepository.GetByStadiumIdAsync(stadiumId);
            var dto = mapper.Map<List<GetReviewDTOs>>(reviews);

            await cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(20));

            return Response<List<GetReviewDTOs>>.Success(dto, "Stadium reviews retrieved successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting reviews for stadium: {StadiumId}", stadiumId);
            return Response<List<GetReviewDTOs>>.Fail("Failed to retrieve reviews.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<List<GetReviewDTOs>>> GetMyReviewsAsync()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Response<List<GetReviewDTOs>>.Fail("User not found in claims.", HttpStatusCode.Unauthorized);

            // Личные отзывы не кешируем — редко запрашиваются
            var reviews = await reviewRepository.GetByUserIdAsync(userId);
            var dto = mapper.Map<List<GetReviewDTOs>>(reviews);
            return Response<List<GetReviewDTOs>>.Success(dto, "Your reviews retrieved successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting user reviews");
            return Response<List<GetReviewDTOs>>.Fail("Failed to retrieve reviews.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<double>> GetAverageRatingAsync(Guid stadiumId)
    {
        try
        {
            var cacheKey = CacheKeys.StadiumRating(stadiumId);
            var cached = await cache.GetAsync<double?>(cacheKey);
            if (cached.HasValue)
            {
                logger.LogInformation("Average rating for stadium {StadiumId} retrieved from cache.", stadiumId);
                return Response<double>.Success(cached.Value, "Average rating retrieved successfully.");
            }

            var rating = await reviewRepository.GetAverageRatingByStadiumIdAsync(stadiumId);
            await cache.SetAsync(cacheKey, rating, TimeSpan.FromMinutes(20));

            return Response<double>.Success(rating, "Average rating retrieved successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting average rating for stadium: {StadiumId}", stadiumId);
            return Response<double>.Fail("Failed to retrieve average rating.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<GetReviewDTOs>> CreateReviewAsync(CreateReviewDTOs dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Response<GetReviewDTOs>.Fail("User not found in claims.", HttpStatusCode.Unauthorized);

            var hasReviewed = await reviewRepository.HasUserReviewedStadiumAsync(userId, dto.StadiumId);
            if (hasReviewed)
                return Response<GetReviewDTOs>.Fail("You have already reviewed this stadium.", HttpStatusCode.Conflict);

            var review = mapper.Map<Review>(dto);
            review.Id = Guid.NewGuid();
            review.UserId = userId;
            review.CreatedAt = DateTime.UtcNow;

            await reviewRepository.CreateAsync(review);

            await cache.RemoveAsync(CacheKeys.StadiumReviews(dto.StadiumId));
            await cache.RemoveAsync(CacheKeys.StadiumRating(dto.StadiumId));

            var result = await reviewRepository.GetByIdAsync(review.Id);
            var resultDto = mapper.Map<GetReviewDTOs>(result);
            return Response<GetReviewDTOs>.Success(resultDto, "Review created successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while creating review");
            return Response<GetReviewDTOs>.Fail("Failed to create review.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<GetReviewDTOs>> UpdateReviewAsync(Guid id, UpdateReviewDTOs dto)
    {
        try
        {
            var review = await reviewRepository.GetByIdAsync(id);
            if (review == null)
                return Response<GetReviewDTOs>.Fail("Review not found.", HttpStatusCode.NotFound);

            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Response<GetReviewDTOs>.Fail("User not found in claims.", HttpStatusCode.Unauthorized);

            if (review.UserId != userId)
                return Response<GetReviewDTOs>.Fail("You are not authorized to update this review.", HttpStatusCode.Forbidden);

            mapper.Map(dto, review);
            await reviewRepository.UpdateAsync(review);

            await cache.RemoveAsync(CacheKeys.StadiumReviews(review.StadiumId));
            await cache.RemoveAsync(CacheKeys.StadiumRating(review.StadiumId));

            var result = await reviewRepository.GetByIdAsync(id);
            var resultDto = mapper.Map<GetReviewDTOs>(result);
            return Response<GetReviewDTOs>.Success(resultDto, "Review updated successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while updating review with id: {Id}", id);
            return Response<GetReviewDTOs>.Fail("Failed to update review.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<bool>> DeleteReviewAsync(Guid id)
    {
        try
        {
            var review = await reviewRepository.GetByIdAsync(id);
            if (review == null)
                return Response<bool>.Fail("Review not found.", HttpStatusCode.NotFound);

            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Response<bool>.Fail("User not found in claims.", HttpStatusCode.Unauthorized);

            if (review.UserId != userId)
                return Response<bool>.Fail("You are not authorized to delete this review.", HttpStatusCode.Forbidden);

            await reviewRepository.DeleteAsync(review);

            await cache.RemoveAsync(CacheKeys.StadiumReviews(review.StadiumId));
            await cache.RemoveAsync(CacheKeys.StadiumRating(review.StadiumId));

            return Response<bool>.Success(true, "Review deleted successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while deleting review with id: {Id}", id);
            return Response<bool>.Fail("Failed to delete review.", HttpStatusCode.InternalServerError);
        }
    }
}