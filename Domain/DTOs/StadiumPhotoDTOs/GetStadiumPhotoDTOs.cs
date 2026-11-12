using Domain.Entities;

namespace Domain.DTOs.StadiumPhotoDTOs;

public class GetStadiumPhotoDTOs
{
    public Guid Id { get; set; }
    public Guid StadiumId { get; set; }
    public string PhotoUrl { get; set; } = null!;
    public bool IsPrimary { get; set; }
    public DateTime CreatedAt { get; set; }
}
