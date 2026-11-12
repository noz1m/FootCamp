using System.ComponentModel.DataAnnotations;

namespace Domain.DTOs.ReviewDTOs;

public class CreateReviewDTOs
{
    [Required(ErrorMessage = "Stadium ID is required.")]
    public Guid StadiumId { get; set; }

    public Guid? BookingId { get; set; }

    [Required(ErrorMessage = "Rating is required.")]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
    public int Rating { get; set; }

    [StringLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters.")]
    public string? Comment { get; set; }
}