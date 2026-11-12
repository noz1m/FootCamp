using System.ComponentModel.DataAnnotations;

namespace Domain.DTOs.PaymentDTOs;

public class CreatePaymentDTOs
{
    [Required(ErrorMessage = "Booking ID is required.")]
    public Guid BookingId { get; set; }

    [Required(ErrorMessage = "Amount is required.")]
    [Range(0.01, 1000000, ErrorMessage = "Amount must be greater than 0.")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Payment method is required.")]
    [RegularExpression("^(card|cash|transfer)$", ErrorMessage = "Payment method must be: card, cash, or transfer.")]
    public string PaymentMethod { get; set; } = null!;
}