using Domain.Entities;

namespace Domain.DTOs.MatchDTOs;

public class GetMatchDTOs
{
    public Guid Id { get; set; }
    public Guid StadiumId { get; set; }
    public string StadiumName { get; set; } = null!;
    public string CreatorId { get; set; } = null!;
    public string CreatorFullName { get; set; } = null!;
    public DateTime MatchTime { get; set; }
    public int MaxPlayers { get; set; }
    public int CurrentPlayers { get; set; }
    public decimal PricePerPlayer { get; set; }
    public string? Description { get; set; }
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
