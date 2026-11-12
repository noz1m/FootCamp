namespace Domain.Entities;

public class Payment
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Booking Booking { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!; // ADD
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = null!; // ADD: card, cash, transfer
    public string? TransactionId { get; set; } // ADD: ID транзакции от платежной системы
    public string Status { get; set; } = null!; // pending, completed, failed, refunded
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // RENAME from PaymentDate
    public DateTime? CompletedAt { get; set; } // ADD: Когда платеж завершен
}