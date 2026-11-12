using Domain.DTOs.ReviewDTOs;
using Infrastructure.Reviews.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/reviews")]
public class ReviewController(
    IReviewService reviewService,
    ILogger<ReviewController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        logger.LogInformation("Getting all reviews");
        var result = await reviewService.GetAllReviewsAsync();
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        logger.LogInformation("Getting review with id: {Id}", id);
        var result = await reviewService.GetReviewByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("stadium/{stadiumId:guid}")]
    public async Task<IActionResult> GetByStadium(Guid stadiumId)
    {
        logger.LogInformation("Getting reviews for stadium: {StadiumId}", stadiumId);
        var result = await reviewService.GetReviewsByStadiumIdAsync(stadiumId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("stadium/{stadiumId:guid}/rating")]
    public async Task<IActionResult> GetAverageRating(Guid stadiumId)
    {
        logger.LogInformation("Getting average rating for stadium: {StadiumId}", stadiumId);
        var result = await reviewService.GetAverageRatingAsync(stadiumId);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpGet("my")]
    public async Task<IActionResult> GetMyReviews()
    {
        logger.LogInformation("Getting current user reviews");
        var result = await reviewService.GetMyReviewsAsync();
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReviewDTOs dto)
    {
        logger.LogInformation("Creating review for stadium: {StadiumId}", dto.StadiumId);
        var result = await reviewService.CreateReviewAsync(dto);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateReviewDTOs dto)
    {
        logger.LogInformation("Updating review with id: {Id}", id);
        var result = await reviewService.UpdateReviewAsync(id, dto);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        logger.LogInformation("Deleting review with id: {Id}", id);
        var result = await reviewService.DeleteReviewAsync(id);
        return StatusCode(result.StatusCode, result);
    }
}