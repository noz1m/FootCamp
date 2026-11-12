namespace Domain.Entities;

public class Match
{
     public Guid Id { get; set; }
    public Guid StadiumId { get; set; }
    public Stadium Stadium { get; set; } = null!;
    public string CreatorId { get; set; } = null!;
    public ApplicationUser Creator { get; set; } = null!;
    public DateTime MatchTime { get; set; }
    public int MaxPlayers { get; set; }
    public int CurrentPlayers { get; set; } = 0; // ADD: Текущее количество игроков
    public decimal PricePerPlayer { get; set; } // ADD: Цена за игрока
    public string Status { get; set; } = null!; // open, full, in_progress, completed, cancelled
    public string? Description { get; set; } // ADD: Описание матча
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // ADD

    // navigation properties
    public List<MatchRequest> Requests { get; set; } = [];
}
