using System.Net;
using System.Security.Claims;
using AutoMapper;
using Domain.ApiResponse;
using Domain.DTOs.MatchDTOs;
using Domain.Entities;
using Domain.Filter;
using Infrastructure.Matches.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Matches.Services;

public class MatchService(
    IMatchRepository matchRepository,
    IHttpContextAccessor httpContextAccessor,
    ILogger<MatchService> logger,
    IMapper mapper) : IMatchService
{
    private string? GetCurrentUserId()
    {
        return httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    public async Task<Response<List<GetMatchDTOs>>> GetAllMatchesAsync()
    {
        try
        {
            var filter = new MatchFilter();
            var result = await matchRepository.GetAllAsync(filter);

            var dto = mapper.Map<List<GetMatchDTOs>>(result.Data);
            return Response<List<GetMatchDTOs>>.Success(dto, "Matches retrieved successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting all matches");
            return Response<List<GetMatchDTOs>>.Fail("Failed to retrieve matches.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<GetMatchDTOs>> GetMatchByIdAsync(Guid id)
    {
        try
        {
            var match = await matchRepository.GetByIdAsync(id);
            if (match == null)
                return Response<GetMatchDTOs>.Fail("Match not found.", HttpStatusCode.NotFound);

            var dto = mapper.Map<GetMatchDTOs>(match);
            return Response<GetMatchDTOs>.Success(dto, "Match retrieved successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting match with id: {Id}", id);
            return Response<GetMatchDTOs>.Fail("Failed to retrieve match.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<List<GetMatchDTOs>>> GetMatchesByStadiumIdAsync(Guid stadiumId)
    {
        try
        {
            var matches = await matchRepository.GetByStadiumIdAsync(stadiumId);

            var dto = mapper.Map<List<GetMatchDTOs>>(matches);
            return Response<List<GetMatchDTOs>>.Success(dto, "Stadium matches retrieved successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting matches for stadium: {StadiumId}", stadiumId);
            return Response<List<GetMatchDTOs>>.Fail("Failed to retrieve matches.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<List<GetMatchDTOs>>> GetUpcomingMatchesAsync()
    {
        try
        {
            var matches = await matchRepository.GetUpcomingAsync();

            var dto = mapper.Map<List<GetMatchDTOs>>(matches);
            return Response<List<GetMatchDTOs>>.Success(dto, "Upcoming matches retrieved successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting upcoming matches");
            return Response<List<GetMatchDTOs>>.Fail("Failed to retrieve matches.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<List<GetMatchDTOs>>> GetPastMatchesAsync()
    {
        try
        {
            var matches = await matchRepository.GetPastAsync();

            var dto = mapper.Map<List<GetMatchDTOs>>(matches);
            return Response<List<GetMatchDTOs>>.Success(dto, "Past matches retrieved successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting past matches");
            return Response<List<GetMatchDTOs>>.Fail("Failed to retrieve matches.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<List<GetMatchDTOs>>> GetMatchesByDateAsync(DateTime date)
    {
        try
        {
            var matches = await matchRepository.GetByDateAsync(date);

            var dto = mapper.Map<List<GetMatchDTOs>>(matches);
            return Response<List<GetMatchDTOs>>.Success(dto, "Matches retrieved successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting matches by date: {Date}", date);
            return Response<List<GetMatchDTOs>>.Fail("Failed to retrieve matches.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<GetMatchDTOs>> CreateMatchAsync(CreateMatchDTOs dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Response<GetMatchDTOs>.Fail("User not found in claims.", HttpStatusCode.Unauthorized);

            var match = mapper.Map<Match>(dto);
            match.Id = Guid.NewGuid();
            match.CreatorId = userId;
            match.CurrentPlayers = 1;
            match.Status = "open";
            match.CreatedAt = DateTime.UtcNow;

            await matchRepository.CreateAsync(match);

            var result = await matchRepository.GetByIdAsync(match.Id);
            var resultDto = mapper.Map<GetMatchDTOs>(result);

            return Response<GetMatchDTOs>.Success(resultDto, "Match created successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while creating match");
            return Response<GetMatchDTOs>.Fail("Failed to create match.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<GetMatchDTOs>> UpdateMatchAsync(Guid id, UpdateMatchDTOs dto)
    {
        try
        {
            var match = await matchRepository.GetByIdAsync(id);
            if (match == null)
                return Response<GetMatchDTOs>.Fail("Match not found.", HttpStatusCode.NotFound);

            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Response<GetMatchDTOs>.Fail("User not found in claims.", HttpStatusCode.Unauthorized);

            if (match.CreatorId != userId)
                return Response<GetMatchDTOs>.Fail("You are not authorized to update this match.", HttpStatusCode.Forbidden);

            mapper.Map(dto, match);
            await matchRepository.UpdateAsync(match);

            var result = await matchRepository.GetByIdAsync(id);
            var resultDto = mapper.Map<GetMatchDTOs>(result);

            return Response<GetMatchDTOs>.Success(resultDto, "Match updated successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while updating match with id: {Id}", id);
            return Response<GetMatchDTOs>.Fail("Failed to update match.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<bool>> UpdateMatchStatusAsync(Guid id, string status)
    {
        try
        {
            var match = await matchRepository.GetByIdAsync(id);
            if (match == null)
                return Response<bool>.Fail("Match not found.", HttpStatusCode.NotFound);

            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Response<bool>.Fail("User not found in claims.", HttpStatusCode.Unauthorized);

            if (match.CreatorId != userId)
                return Response<bool>.Fail("You are not authorized to update this match.", HttpStatusCode.Forbidden);

            match.Status = status;
            await matchRepository.UpdateAsync(match);

            return Response<bool>.Success(true, "Match status updated successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while updating match status with id: {Id}", id);
            return Response<bool>.Fail("Failed to update match status.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<bool>> DeleteMatchAsync(Guid id)
    {
        try
        {
            var match = await matchRepository.GetByIdAsync(id);
            if (match == null)
                return Response<bool>.Fail("Match not found.", HttpStatusCode.NotFound);

            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Response<bool>.Fail("User not found in claims.", HttpStatusCode.Unauthorized);

            if (match.CreatorId != userId)
                return Response<bool>.Fail("You are not authorized to delete this match.", HttpStatusCode.Forbidden);

            await matchRepository.DeleteAsync(match);

            return Response<bool>.Success(true, "Match deleted successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while deleting match with id: {Id}", id);
            return Response<bool>.Fail("Failed to delete match.", HttpStatusCode.InternalServerError);
        }
    }
}