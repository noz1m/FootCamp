namespace Domain.Entities;

public class MatchRequest
{
    public Guid Id { get; set; }
    public Guid MatchId { get; set; }
    public Match Match { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
    public string Status { get; set; } = null!; // pending, approved, rejected, cancelled
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // ADD
    public DateTime? ResponsedAt { get; set; } // ADD: Когда был ответ на запрос
}