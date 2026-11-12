namespace Domain.Entities;

public class StadiumPhoto
{
    public Guid Id { get; set; }
    public Guid StadiumId { get; set; }
    public Stadium Stadium { get; set; } = null!;
    public string PhotoUrl { get; set; } = null!;
    public bool IsPrimary { get; set; } = false; // ADD: Главное фото
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // ADD
}