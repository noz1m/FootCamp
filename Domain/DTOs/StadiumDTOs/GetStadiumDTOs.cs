namespace Domain.DTOs.StadiumDTOs;

public class GetStadiumDTOs
{
     public Guid Id { get; set; }
    public string OwnerId { get; set; } = null!;
    public string OwnerFullName { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string City { get; set; } = null!;
    public decimal PricePerHour { get; set; }
    public int Capacity { get; set; }
    public string? Facilities { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public bool IsActive { get; set; }
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public List<string> PhotoUrls { get; set; } = [];
    public DateTime CreatedAt { get; set; }
}
