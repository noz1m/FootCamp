using Domain.ApiResponse;
using Domain.Entities;
using Domain.Pagination;
using Domain.Filter;
using Infrastructure.Data;
using Infrastructure.Matches.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Matches.Repositories;

public class MatchRepository(
    DataContext context,
    ILogger<MatchRepository> logger,
    Pagination<Match> pagination) : IMatchRepository
{
    public async Task<PagedResponse<Match>> GetAllAsync(MatchFilter filter)
    {
        try
        {
            var query = context.Matches
                .Include(m => m.Stadium)
                .Include(m => m.Creator)
                .Include(m => m.Requests)
                .AsQueryable();

            if (filter.StadiumId != null)
                query = query.Where(m => m.StadiumId == filter.StadiumId);

            if (filter.CreatorId != null)
                query = query.Where(m => m.CreatorId == filter.CreatorId);

            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(m => m.Status.ToLower() == filter.Status.ToLower());

            if (filter.MinPlayers != null)
                query = query.Where(m => m.MaxPlayers >= filter.MinPlayers);

            if (filter.MaxPlayers != null)
                query = query.Where(m => m.MaxPlayers <= filter.MaxPlayers);

            if (filter.From != null)
                query = query.Where(m => m.MatchTime >= filter.From);

            if (filter.To != null)
                query = query.Where(m => m.MatchTime <= filter.To);

            return await pagination.GetPagedAsync(query, filter.PageNumber, filter.PageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting all matches");
            throw;
        }
    }

    public async Task<Match?> GetByIdAsync(Guid id)
    {
        try
        {
            return await context.Matches
                .Include(m => m.Stadium)
                .Include(m => m.Creator)
                .Include(m => m.Requests)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting match with id: {Id}", id);
            throw;
        }
    }

    public async Task<List<Match>> GetByStadiumIdAsync(Guid stadiumId)
    {
        try
        {
            return await context.Matches
                .Include(m => m.Creator)
                .Include(m => m.Requests)
                .Where(m => m.StadiumId == stadiumId)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting matches by stadiumId: {StadiumId}", stadiumId);
            throw;
        }
    }

    public async Task<List<Match>> GetByCreatorIdAsync(string creatorId)
    {
        try
        {
            return await context.Matches
                .Include(m => m.Stadium)
                .Include(m => m.Requests)
                .Where(m => m.CreatorId == creatorId)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting matches by creatorId: {CreatorId}", creatorId);
            throw;
        }
    }

    public async Task<List<Match>> GetUpcomingAsync()
    {
        try
        {
            return await context.Matches
                .Include(m => m.Stadium)
                .Include(m => m.Creator)
                .Where(m => m.MatchTime > DateTime.UtcNow && m.Status != "cancelled")
                .OrderBy(m => m.MatchTime)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting upcoming matches");
            throw;
        }
    }

    public async Task<List<Match>> GetPastAsync()
    {
        try
        {
            return await context.Matches
                .Include(m => m.Stadium)
                .Include(m => m.Creator)
                .Where(m => m.MatchTime < DateTime.UtcNow)
                .OrderByDescending(m => m.MatchTime)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting past matches");
            throw;
        }
    }

    public async Task<List<Match>> GetByDateAsync(DateTime date)
    {
        try
        {
            return await context.Matches
                .Include(m => m.Stadium)
                .Include(m => m.Creator)
                .Where(m => m.MatchTime.Date == date.Date)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting matches by date: {Date}", date);
            throw;
        }
    }

    public async Task<List<Match>> GetByStatusAsync(string status)
    {
        try
        {
            return await context.Matches
                .Include(m => m.Stadium)
                .Include(m => m.Creator)
                .Where(m => m.Status.ToLower() == status.ToLower())
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting matches by status: {Status}", status);
            throw;
        }
    }

    public async Task<int> CreateAsync(Match match)
    {
        try
        {
            await context.Matches.AddAsync(match);
            return await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while creating match");
            throw;
        }
    }

    public async Task<int> UpdateAsync(Match match)
    {
        try
        {
            context.Matches.Update(match);
            return await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while updating match with id: {Id}", match.Id);
            throw;
        }
    }

    public async Task<int> DeleteAsync(Match match)
    {
        try
        {
            context.Matches.Remove(match);
            return await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while deleting match with id: {Id}", match.Id);
            throw;
        }
    }
}