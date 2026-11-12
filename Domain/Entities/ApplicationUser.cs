using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = null!;
    public bool IsActive { get; set; } = true; 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    
    // navigation properties
    public List<Stadium> OwnedStadiums { get; set; } = [];
    public List<Booking> Bookings { get; set; } = [];
    public List<Review> Reviews { get; set; } = [];
    public List<Match> CreatedMatches { get; set; } = [];
    public List<MatchRequest> MatchRequests { get; set; } = [];
    public List<Payment> Payments { get; set; } = [];
}