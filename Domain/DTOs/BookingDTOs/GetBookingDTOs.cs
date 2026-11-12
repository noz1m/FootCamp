using Domain.Entities;

namespace Domain.DTOs.BookingDTOs;

public class GetBookingDTOs
{
    public Guid Id { get; set; }
    public Guid StadiumId { get; set; }
    public string StadiumName { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public string UserFullName { get; set; } = null!;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal PrepaymentAmount { get; set; }
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
