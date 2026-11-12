namespace Domain.DTOs.MatchDTOs;

public class UpdateMatchDTOs
{
    public DateTime? MatchTime { get; set; }
    public int? MaxPlayers { get; set; }
    public decimal? PricePerPlayer { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }  // open, full, in_progress, completed, cancelled
}
