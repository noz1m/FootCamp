using System.ComponentModel.DataAnnotations;

namespace Domain.DTOs.BookingDTOs;

public class CreateBookingDTOs
{
    [Required(ErrorMessage = "Stadium ID is required.")]
    public Guid StadiumId { get; set; }

    [Required(ErrorMessage = "Start time is required.")]
    public DateTime StartTime { get; set; }

    [Required(ErrorMessage = "End time is required.")]
    public DateTime EndTime { get; set; }

    [Range(0, 1000000, ErrorMessage = "Prepayment amount must be 0 or greater.")]
    public decimal PrepaymentAmount { get; set; }
}