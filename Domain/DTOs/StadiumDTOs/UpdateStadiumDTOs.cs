namespace Domain.DTOs.StadiumDTOs;

public class UpdateStadiumDTOs
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public decimal? PricePerHour { get; set; }
    public int? Capacity { get; set; }
    public string? Facilities { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool? IsActive { get; set; }
}
