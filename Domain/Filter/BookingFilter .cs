using Domain.Pagination;

namespace Domain.Filter;

public class BookingFilter : ValidFilter
{
    public string? UserId { get; set; }
    public Guid? StadiumId { get; set; }
    public string? Status { get; set; }    // pending, confirmed, cancelled, completed
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}