using Domain.Pagination;

namespace Domain.Filter;

public class StadiumFilter : ValidFilter
{
    public string? Name { get; set; }
    public string? Location { get; set; }
    public int? MinCapacity { get; set; }
    public int? MaxCapacity { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? RadiusKm { get; set; }
    public double? MinRating { get; set; }
    public string? OwnerEmail { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public bool? IsActive { get; set; }
}