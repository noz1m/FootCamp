using Domain.DTOs.MatchRequestDTOs;
using Infrastructure.MatchRequests.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/match-requests")]
[Authorize]
public class MatchRequestController(
    IMatchRequestService matchRequestService,
    ILogger<MatchRequestController> logger) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        logger.LogInformation("Admin: getting all match requests");
        var result = await matchRequestService.GetAllMatchRequestsAsync();
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        logger.LogInformation("Getting match request with id: {Id}", id);
        var result = await matchRequestService.GetMatchRequestByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyRequests()
    {
        logger.LogInformation("Getting match requests for current user");
        var result = await matchRequestService.GetMyMatchRequestsAsync();
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("match/{matchId:guid}")]
    public async Task<IActionResult> GetByMatch(Guid matchId)
    {
        logger.LogInformation("Getting requests for match: {MatchId}", matchId);
        var result = await matchRequestService.GetMatchRequestsByMatchIdAsync(matchId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("match/{matchId:guid}/pending")]
    public async Task<IActionResult> GetPending(Guid matchId)
    {
        logger.LogInformation("Getting pending requests for match: {MatchId}", matchId);
        var result = await matchRequestService.GetPendingRequestsAsync(matchId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMatchRequestDTOs dto)
    {
        logger.LogInformation("Creating match request for match: {MatchId}", dto.MatchId);
        var result = await matchRequestService.CreateMatchRequestAsync(dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMatchRequestDTOs dto)
    {
        logger.LogInformation("Updating match request with id: {Id}", id);
        var result = await matchRequestService.UpdateMatchRequestAsync(id, dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        logger.LogInformation("Approving match request with id: {Id}", id);
        var result = await matchRequestService.ApproveMatchRequestAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id)
    {
        logger.LogInformation("Rejecting match request with id: {Id}", id);
        var result = await matchRequestService.RejectMatchRequestAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        logger.LogInformation("Deleting match request with id: {Id}", id);
        var result = await matchRequestService.DeleteMatchRequestAsync(id);
        return StatusCode(result.StatusCode, result);
    }
}