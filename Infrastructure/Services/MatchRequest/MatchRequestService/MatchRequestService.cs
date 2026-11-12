using System.Net;
using System.Security.Claims;
using AutoMapper;
using Domain.ApiResponse;
using Domain.DTOs.MatchRequestDTOs;
using Domain.Entities;
using Domain.Filter;
using Infrastructure.MatchRequests.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Infrastructure.MatchRequests.Services;

public class MatchRequestService(
    IMatchRequestRepository matchRequestRepository,
    IHttpContextAccessor httpContextAccessor,
    ILogger<MatchRequestService> logger,
    IMapper mapper) : IMatchRequestService
{
    private string? GetCurrentUserId()
    {
        return httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    public async Task<Response<List<GetMatchRequestDTOs>>> GetAllMatchRequestsAsync()
    {
        try
        {
            var filter = new MatchRequestFilter();
            var result = await matchRequestRepository.GetAllAsync(filter);

            var dto = mapper.Map<List<GetMatchRequestDTOs>>(result.Data);
            return Response<List<GetMatchRequestDTOs>>.Success(dto, "Match requests retrieved successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting all match requests");
            return Response<List<GetMatchRequestDTOs>>.Fail("Failed to retrieve match requests.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<GetMatchRequestDTOs>> GetMatchRequestByIdAsync(Guid id)
    {
        try
        {
            var matchRequest = await matchRequestRepository.GetByIdAsync(id);
            if (matchRequest == null)
                return Response<GetMatchRequestDTOs>.Fail("Match request not found.", HttpStatusCode.NotFound);

            var dto = mapper.Map<GetMatchRequestDTOs>(matchRequest);
            return Response<GetMatchRequestDTOs>.Success(dto, "Match request retrieved successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting match request with id: {Id}", id);
            return Response<GetMatchRequestDTOs>.Fail("Failed to retrieve match request.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<List<GetMatchRequestDTOs>>> GetMyMatchRequestsAsync()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Response<List<GetMatchRequestDTOs>>.Fail("User not found in claims.", HttpStatusCode.Unauthorized);

            var requests = await matchRequestRepository.GetByUserIdAsync(userId);

            var dto = mapper.Map<List<GetMatchRequestDTOs>>(requests);
            return Response<List<GetMatchRequestDTOs>>.Success(dto, "Your match requests retrieved successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting user match requests");
            return Response<List<GetMatchRequestDTOs>>.Fail("Failed to retrieve match requests.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<List<GetMatchRequestDTOs>>> GetMatchRequestsByMatchIdAsync(Guid matchId)
    {
        try
        {
            var requests = await matchRequestRepository.GetByMatchIdAsync(matchId);

            var dto = mapper.Map<List<GetMatchRequestDTOs>>(requests);
            return Response<List<GetMatchRequestDTOs>>.Success(dto, "Match requests retrieved successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting requests for match: {MatchId}", matchId);
            return Response<List<GetMatchRequestDTOs>>.Fail("Failed to retrieve match requests.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<List<GetMatchRequestDTOs>>> GetPendingRequestsAsync(Guid matchId)
    {
        try
        {
            var requests = await matchRequestRepository.GetPendingByMatchIdAsync(matchId);

            var dto = mapper.Map<List<GetMatchRequestDTOs>>(requests);
            return Response<List<GetMatchRequestDTOs>>.Success(dto, "Pending requests retrieved successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting pending requests for match: {MatchId}", matchId);
            return Response<List<GetMatchRequestDTOs>>.Fail("Failed to retrieve pending requests.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<GetMatchRequestDTOs>> CreateMatchRequestAsync(CreateMatchRequestDTOs dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Response<GetMatchRequestDTOs>.Fail("User not found in claims.", HttpStatusCode.Unauthorized);

            var hasRequested = await matchRequestRepository.HasUserRequestedAsync(userId, dto.MatchId);
            if (hasRequested)
                return Response<GetMatchRequestDTOs>.Fail("You have already requested to join this match.", HttpStatusCode.Conflict);

            var matchRequest = mapper.Map<MatchRequest>(dto);
            matchRequest.Id = Guid.NewGuid();
            matchRequest.UserId = userId;
            matchRequest.Status = "pending";
            matchRequest.CreatedAt = DateTime.UtcNow;

            await matchRequestRepository.CreateAsync(matchRequest);

            var result = await matchRequestRepository.GetByIdAsync(matchRequest.Id);
            var resultDto = mapper.Map<GetMatchRequestDTOs>(result);

            return Response<GetMatchRequestDTOs>.Success(resultDto, "Match request created successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while creating match request");
            return Response<GetMatchRequestDTOs>.Fail("Failed to create match request.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<GetMatchRequestDTOs>> UpdateMatchRequestAsync(Guid id, UpdateMatchRequestDTOs dto)
    {
        try
        {
            var matchRequest = await matchRequestRepository.GetByIdAsync(id);
            if (matchRequest == null)
                return Response<GetMatchRequestDTOs>.Fail("Match request not found.", HttpStatusCode.NotFound);

            mapper.Map(dto, matchRequest);
            await matchRequestRepository.UpdateAsync(matchRequest);

            var result = await matchRequestRepository.GetByIdAsync(id);
            var resultDto = mapper.Map<GetMatchRequestDTOs>(result);

            return Response<GetMatchRequestDTOs>.Success(resultDto, "Match request updated successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while updating match request with id: {Id}", id);
            return Response<GetMatchRequestDTOs>.Fail("Failed to update match request.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<bool>> ApproveMatchRequestAsync(Guid id)
    {
        try
        {
            var matchRequest = await matchRequestRepository.GetByIdAsync(id);
            if (matchRequest == null)
                return Response<bool>.Fail("Match request not found.", HttpStatusCode.NotFound);

            matchRequest.Status = "approved";
            matchRequest.ResponsedAt = DateTime.UtcNow;
            await matchRequestRepository.UpdateAsync(matchRequest);

            return Response<bool>.Success(true, "Match request approved successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while approving match request with id: {Id}", id);
            return Response<bool>.Fail("Failed to approve match request.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<bool>> RejectMatchRequestAsync(Guid id)
    {
        try
        {
            var matchRequest = await matchRequestRepository.GetByIdAsync(id);
            if (matchRequest == null)
                return Response<bool>.Fail("Match request not found.", HttpStatusCode.NotFound);

            matchRequest.Status = "rejected";
            matchRequest.ResponsedAt = DateTime.UtcNow;
            await matchRequestRepository.UpdateAsync(matchRequest);

            return Response<bool>.Success(true, "Match request rejected successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while rejecting match request with id: {Id}", id);
            return Response<bool>.Fail("Failed to reject match request.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response<bool>> DeleteMatchRequestAsync(Guid id)
    {
        try
        {
            var matchRequest = await matchRequestRepository.GetByIdAsync(id);
            if (matchRequest == null)
                return Response<bool>.Fail("Match request not found.", HttpStatusCode.NotFound);

            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Response<bool>.Fail("User not found in claims.", HttpStatusCode.Unauthorized);

            if (matchRequest.UserId != userId)
                return Response<bool>.Fail("You are not authorized to delete this match request.", HttpStatusCode.Forbidden);

            await matchRequestRepository.DeleteAsync(matchRequest);

            return Response<bool>.Success(true, "Match request deleted successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while deleting match request with id: {Id}", id);
            return Response<bool>.Fail("Failed to delete match request.", HttpStatusCode.InternalServerError);
        }
    }
}