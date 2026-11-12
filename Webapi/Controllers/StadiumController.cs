using Domain.DTOs.StadiumDTOs;
using Infrastructure.Stadiums.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/stadiums")]
public class StadiumController(
    IStadiumService stadiumService,
    ILogger<StadiumController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        logger.LogInformation("Getting all stadiums");
        var result = await stadiumService.GetAllStadiumsAsync();
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        logger.LogInformation("Getting stadium with id: {Id}", id);
        var result = await stadiumService.GetStadiumByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpGet("my")]
    public async Task<IActionResult> GetMyStadiums()
    {
        logger.LogInformation("Getting stadiums for current owner");
        var result = await stadiumService.GetMyStadiumsAsync();
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string searchTerm)
    {
        logger.LogInformation("Searching stadiums: {SearchTerm}", searchTerm);
        var result = await stadiumService.SearchStadiumsAsync(searchTerm);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("available")]
    public async Task<IActionResult> GetAvailable(
        [FromQuery] DateTime startTime,
        [FromQuery] DateTime endTime)
    {
        logger.LogInformation("Getting available stadiums from {Start} to {End}", startTime, endTime);
        var result = await stadiumService.GetAvailableStadiumsAsync(startTime, endTime);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("by-location")]
    public async Task<IActionResult> GetByLocation([FromQuery] string location)
    {
        logger.LogInformation("Getting stadiums by location: {Location}", location);
        var result = await stadiumService.GetStadiumsByLocationAsync(location);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("by-price")]
    public async Task<IActionResult> GetByPriceRange(
        [FromQuery] decimal minPrice,
        [FromQuery] decimal maxPrice)
    {
        logger.LogInformation("Getting stadiums by price: {Min}-{Max}", minPrice, maxPrice);
        var result = await stadiumService.GetStadiumsByPriceRangeAsync(minPrice, maxPrice);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = "Manager")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStadiumDTOs dto)
    {
        logger.LogInformation("Creating stadium: {@Dto}", dto);
        var result = await stadiumService.CreateStadiumAsync(dto);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = "Manager")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStadiumDTOs dto)
    {
        logger.LogInformation("Updating stadium with id: {Id}", id);
        var result = await stadiumService.UpdateStadiumAsync(id, dto);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = "Manager,Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        logger.LogInformation("Deleting stadium with id: {Id}", id);
        var result = await stadiumService.DeleteStadiumAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id)
    {
        logger.LogInformation("Activating stadium with id: {Id}", id);
        var result = await stadiumService.ActivateStadiumAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = "Manager,Admin")]
    [HttpPut("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        logger.LogInformation("Deactivating stadium with id: {Id}", id);
        var result = await stadiumService.DeactivateStadiumAsync(id);
        return StatusCode(result.StatusCode, result);
    }
}