using Domain.Entities;

namespace Domain.DTOs.ReviewDTOs;

public class GetReviewDTOs
{
    public Guid Id { get; set; }
    public Guid StadiumId { get; set; }
    public string StadiumName { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public string UserFullName { get; set; } = null!;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}