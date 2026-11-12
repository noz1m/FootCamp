using Infrastructure.StadiumPhotos.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/stadiums/{stadiumId:guid}/photos")]
public class StadiumPhotoController(
    IStadiumPhotoService stadiumPhotoService,
    ILogger<StadiumPhotoController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetByStadium(Guid stadiumId)
    {
        logger.LogInformation("Getting photos for stadium: {StadiumId}", stadiumId);
        var result = await stadiumPhotoService.GetPhotosByStadiumIdAsync(stadiumId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{photoId:guid}")]
    public async Task<IActionResult> GetById(Guid stadiumId, Guid photoId)
    {
        logger.LogInformation("Getting photo {PhotoId} for stadium {StadiumId}", photoId, stadiumId);
        var result = await stadiumPhotoService.GetPhotoByIdAsync(photoId);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = "Manager")]
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(Guid stadiumId, IFormFile file)
    {
        logger.LogInformation("Uploading photo for stadium: {StadiumId}", stadiumId);
        var result = await stadiumPhotoService.UploadPhotoAsync(stadiumId, file);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = "Manager")]
    [HttpPost("upload-multiple")]
    public async Task<IActionResult> UploadMultiple(Guid stadiumId, List<IFormFile> files)
    {
        logger.LogInformation("Uploading {Count} photos for stadium: {StadiumId}", files.Count, stadiumId);
        var result = await stadiumPhotoService.UploadMultiplePhotosAsync(stadiumId, files);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = "Manager")]
    [HttpPut("{photoId:guid}/set-primary")]
    public async Task<IActionResult> SetPrimary(Guid stadiumId, Guid photoId)
    {
        logger.LogInformation("Setting primary photo {PhotoId} for stadium {StadiumId}", photoId, stadiumId);
        var result = await stadiumPhotoService.SetPrimaryPhotoAsync(photoId);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = "Manager,Admin")]
    [HttpDelete("{photoId:guid}")]
    public async Task<IActionResult> Delete(Guid stadiumId, Guid photoId)
    {
        logger.LogInformation("Deleting photo {PhotoId} for stadium {StadiumId}", photoId, stadiumId);
        var result = await stadiumPhotoService.DeletePhotoAsync(photoId);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = "Manager,Admin")]
    [HttpDelete("all")]
    public async Task<IActionResult> DeleteAll(Guid stadiumId)
    {
        logger.LogInformation("Deleting all photos for stadium: {StadiumId}", stadiumId);
        var result = await stadiumPhotoService.DeleteAllPhotosAsync(stadiumId);
        return StatusCode(result.StatusCode, result);
    }
}
