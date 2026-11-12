using Domain.Entities;

namespace Domain.DTOs.PaymentDTOs;

public class GetPaymentDTOs
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public string UserId { get; set; } = null!;
    public string UserFullName { get; set; } = null!;
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = null!;
    public string? TransactionId { get; set; }
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
