namespace Domain.Entities;

public class Review
{
    public Guid Id { get; set; }
    public Guid StadiumId { get; set; }
    public Stadium Stadium { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
    public Guid? BookingId { get; set; } // ADD: Связь с бронированием
    public Booking? Booking { get; set; } // ADD
    public int Rating { get; set; } // 1-5
    public string? Comment { get; set; } // CHANGE: может быть null
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; } // ADD: Если пользователь редактирует отзыв
}