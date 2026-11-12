using Domain.ApiResponse;
using Domain.Entities;
using Domain.Pagination;
using Domain.Filter;
using Infrastructure.Data;
using Infrastructure.MatchRequests.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.MatchRequests.Repositories;

public class MatchRequestRepository(
    DataContext context,
    ILogger<MatchRequestRepository> logger,
    Pagination<MatchRequest> pagination) : IMatchRequestRepository
{
    public async Task<PagedResponse<MatchRequest>> GetAllAsync(MatchRequestFilter filter)
    {
        try
        {
            var query = context.MatchRequests
                .Include(mr => mr.Match)
                .Include(mr => mr.User)
                .AsQueryable();

            if (filter.MatchId != null)
                query = query.Where(mr => mr.MatchId == filter.MatchId);

            if (filter.UserId != null)
                query = query.Where(mr => mr.UserId == filter.UserId);

            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(mr => mr.Status.ToLower() == filter.Status.ToLower());

            return await pagination.GetPagedAsync(query, filter.PageNumber, filter.PageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting all match requests");
            throw;
        }
    }

    public async Task<MatchRequest?> GetByIdAsync(Guid id)
    {
        try
        {
            return await context.MatchRequests
                .Include(mr => mr.Match)
                .Include(mr => mr.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(mr => mr.Id == id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting match request with id: {Id}", id);
            throw;
        }
    }

    public async Task<List<MatchRequest>> GetByMatchIdAsync(Guid matchId)
    {
        try
        {
            return await context.MatchRequests
                .Include(mr => mr.User)
                .Where(mr => mr.MatchId == matchId)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting match requests by matchId: {MatchId}", matchId);
            throw;
        }
    }

    public async Task<List<MatchRequest>> GetByUserIdAsync(string userId)
    {
        try
        {
            return await context.MatchRequests
                .Include(mr => mr.Match)
                .Where(mr => mr.UserId == userId)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting match requests by userId: {UserId}", userId);
            throw;
        }
    }

    public async Task<List<MatchRequest>> GetPendingByMatchIdAsync(Guid matchId)
    {
        try
        {
            return await context.MatchRequests
                .Include(mr => mr.User)
                .Where(mr => mr.MatchId == matchId && mr.Status == "pending")
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting pending requests for matchId: {MatchId}", matchId);
            throw;
        }
    }

    public async Task<bool> HasUserRequestedAsync(string userId, Guid matchId)
    {
        try
        {
            return await context.MatchRequests
                .AnyAsync(mr => mr.UserId == userId && mr.MatchId == matchId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while checking user request for matchId: {MatchId}", matchId);
            throw;
        }
    }

    public async Task<int> CreateAsync(MatchRequest matchRequest)
    {
        try
        {
            await context.MatchRequests.AddAsync(matchRequest);
            return await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while creating match request");
            throw;
        }
    }

    public async Task<int> UpdateAsync(MatchRequest matchRequest)
    {
        try
        {
            matchRequest.ResponsedAt = DateTime.UtcNow;
            context.MatchRequests.Update(matchRequest);
            return await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while updating match request with id: {Id}", matchRequest.Id);
            throw;
        }
    }

    public async Task<int> DeleteAsync(MatchRequest matchRequest)
    {
        try
        {
            context.MatchRequests.Remove(matchRequest);
            return await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while deleting match request with id: {Id}", matchRequest.Id);
            throw;
        }
    }
}