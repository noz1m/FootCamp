namespace Domain.DTOs.BookingDTOs;

public class UpdateBookingDTOs
{
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public decimal? PrepaymentAmount { get; set; }
    public string? Status { get; set; }  // pending, confirmed, cancelled, completed
}
