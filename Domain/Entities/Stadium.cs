namespace Domain.Entities;

public class Stadium
{
    public Guid Id { get; set; }
    public string OwnerId { get; set; } = null!;
    public ApplicationUser Owner { get; set; } = null!; // ADD: Navigation property
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Address { get; set; } = null!; // ADD: Полный адрес
    public string City { get; set; } = null!; // ADD: Город
    public decimal PricePerHour { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public bool IsActive { get; set; } = true; // ADD: Активен ли стадион
    public int Capacity { get; set; } // ADD: Вместимость
    public string? Facilities { get; set; } // ADD: Удобства (раздевалка, душ и т.д.)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; } // ADD

    // navigation properties
    public List<StadiumPhoto> Photos { get; set; } = [];
    public List<Booking> Bookings { get; set; } = [];
    public List<Review> Reviews { get; set; } = [];
    public List<Match> Matches { get; set; } = []; // ADD: Матчи на стадионе
}