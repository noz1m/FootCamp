using System.ComponentModel.DataAnnotations;

namespace Domain.DTOs.StadiumDTOs;

public class CreateStadiumDTOs
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters.")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 1000 characters.")]
    public string Description { get; set; } = null!;

    [Required(ErrorMessage = "Address is required.")]
    [StringLength(200, MinimumLength = 5, ErrorMessage = "Address must be between 5 and 200 characters.")]
    public string Address { get; set; } = null!;

    [Required(ErrorMessage = "City is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "City must be between 2 and 100 characters.")]
    public string City { get; set; } = null!;

    [Required(ErrorMessage = "Price per hour is required.")]
    [Range(0.01, 1000000, ErrorMessage = "Price must be greater than 0.")]
    public decimal PricePerHour { get; set; }

    [Required(ErrorMessage = "Capacity is required.")]
    [Range(1, 10000, ErrorMessage = "Capacity must be between 1 and 10000.")]
    public int Capacity { get; set; }

    [StringLength(500, ErrorMessage = "Facilities cannot exceed 500 characters.")]
    public string? Facilities { get; set; }

    [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90.")]
    public double Latitude { get; set; }

    [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180.")]
    public double Longitude { get; set; }
}
