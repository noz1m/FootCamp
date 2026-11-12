using System.ComponentModel.DataAnnotations;

namespace Domain.DTOs.MatchDTOs;

public class CreateMatchDTOs
{
    [Required(ErrorMessage = "Stadium ID is required.")]
    public Guid StadiumId { get; set; }

    [Required(ErrorMessage = "Match time is required.")]
    public DateTime MatchTime { get; set; }

    [Required(ErrorMessage = "Max players is required.")]
    [Range(2, 100, ErrorMessage = "Max players must be between 2 and 100.")]
    public int MaxPlayers { get; set; }

    [Range(0, 1000000, ErrorMessage = "Price per player must be 0 or greater.")]
    public decimal PricePerPlayer { get; set; }

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    public string? Description { get; set; }
}