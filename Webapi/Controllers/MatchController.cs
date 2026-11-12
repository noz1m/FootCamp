using Domain.DTOs.MatchDTOs;
using Infrastructure.Matches.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/matches")]
public class MatchController(
    IMatchService matchService,
    ILogger<MatchController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        logger.LogInformation("Getting all matches");
        var result = await matchService.GetAllMatchesAsync();
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        logger.LogInformation("Getting match with id: {Id}", id);
        var result = await matchService.GetMatchByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("stadium/{stadiumId:guid}")]
    public async Task<IActionResult> GetByStadium(Guid stadiumId)
    {
        logger.LogInformation("Getting matches for stadium: {StadiumId}", stadiumId);
        var result = await matchService.GetMatchesByStadiumIdAsync(stadiumId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("upcoming")]
    public async Task<IActionResult> GetUpcoming()
    {
        logger.LogInformation("Getting upcoming matches");
        var result = await matchService.GetUpcomingMatchesAsync();
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("past")]
    public async Task<IActionResult> GetPast()
    {
        logger.LogInformation("Getting past matches");
        var result = await matchService.GetPastMatchesAsync();
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("by-date")]
    public async Task<IActionResult> GetByDate([FromQuery] DateTime date)
    {
        logger.LogInformation("Getting matches by date: {Date}", date);
        var result = await matchService.GetMatchesByDateAsync(date);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMatchDTOs dto)
    {
        logger.LogInformation("Creating match at stadium: {StadiumId}", dto.StadiumId);
        var result = await matchService.CreateMatchAsync(dto);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMatchDTOs dto)
    {
        logger.LogInformation("Updating match with id: {Id}", id);
        var result = await matchService.UpdateMatchAsync(id, dto);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromQuery] string status)
    {
        logger.LogInformation("Updating status for match {Id} to {Status}", id, status);
        var result = await matchService.UpdateMatchStatusAsync(id, status);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        logger.LogInformation("Deleting match with id: {Id}", id);
        var result = await matchService.DeleteMatchAsync(id);
        return StatusCode(result.StatusCode, result);
    }
}