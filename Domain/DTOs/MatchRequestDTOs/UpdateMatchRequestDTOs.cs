namespace Domain.DTOs.MatchRequestDTOs;

public class UpdateMatchRequestDTOs
{
    public string Status { get; set; } = null!;  // pending, approved, rejected, cancelled
}
