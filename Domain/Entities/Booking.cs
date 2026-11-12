namespace Domain.Entities;

public class Booking
{
    public Guid Id { get; set; }
    public Guid StadiumId { get; set; }
    public Stadium Stadium { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal TotalPrice { get; set; } // ADD: Общая стоимость бронирования
    public decimal PrepaymentAmount { get; set; }
    public string Status { get; set; } = null!; // pending, confirmed, cancelled, completed
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; } // ADD: Для отслеживания изменений
    public Payment? Payment { get; set; }
}
