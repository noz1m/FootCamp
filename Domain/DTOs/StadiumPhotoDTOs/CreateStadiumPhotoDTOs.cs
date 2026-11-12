using System.ComponentModel.DataAnnotations;

namespace Domain.DTOs.StadiumPhotoDTOs;

public class CreateStadiumPhotoDTOs
{
    [Required(ErrorMessage = "Stadium ID is required.")]
    public Guid StadiumId { get; set; }

    [Required(ErrorMessage = "Photo URL is required.")]
    [Url(ErrorMessage = "Invalid URL format.")]
    [StringLength(500, ErrorMessage = "Photo URL cannot exceed 500 characters.")]
    public string PhotoUrl { get; set; } = null!;

    public bool IsPrimary { get; set; } = false;
}