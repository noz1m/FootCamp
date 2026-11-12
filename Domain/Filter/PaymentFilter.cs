using Domain.Pagination;

namespace Domain.Filter;

public class PaymentFilter : ValidFilter
{
    public Guid? BookingId { get; set; }
    public string? UserId { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public string? Status { get; set; }    // pending, completed, failed, refunded
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}