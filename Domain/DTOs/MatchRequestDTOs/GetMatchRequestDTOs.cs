using Domain.Entities;

namespace Domain.DTOs.MatchRequestDTOs;

public class GetMatchRequestDTOs
{
    public Guid Id { get; set; }
    public Guid MatchId { get; set; }
    public DateTime MatchTime { get; set; }
    public string UserId { get; set; } = null!;
    public string UserFullName { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? ResponsedAt { get; set; }
}
