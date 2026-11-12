namespace Domain.DTOs.PaymentDTOs;

public class UpdatePaymentDTOs
{
    public string? Status { get; set; }         // pending, completed, failed, refunded
    public string? TransactionId { get; set; }
}
