using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class DataContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    public DataContext(DbContextOptions<DataContext> options) : base(options) {}

    public DbSet<Booking> Bookings { get; set; } = null!;
    public DbSet<Match> Matches { get; set; } = null!;
    public DbSet<MatchRequest> MatchRequests { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<Review> Reviews { get; set; } = null!;
    public DbSet<Stadium> Stadiums { get; set; } = null!;
    public DbSet<StadiumPhoto> StadiumPhotos { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Booking
        builder.Entity<Booking>(entity =>
        {
            entity.HasKey(b => b.Id);

            entity.HasOne(b => b.Stadium)
                .WithMany(s => s.Bookings)
                .HasForeignKey(b => b.StadiumId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(b => b.TotalPrice)
                .HasColumnType("decimal(18,2)");

            entity.Property(b => b.PrepaymentAmount)
                .HasColumnType("decimal(18,2)");
        });

        // Match
        builder.Entity<Match>(entity =>
        {
            entity.HasKey(m => m.Id);

            entity.HasOne(m => m.Stadium)
                .WithMany(s => s.Matches)
                .HasForeignKey(m => m.StadiumId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(m => m.Creator)
                .WithMany(u => u.CreatedMatches)
                .HasForeignKey(m => m.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(m => m.PricePerPlayer)
                .HasColumnType("decimal(18,2)");
        });

        // MatchRequest
        builder.Entity<MatchRequest>(entity =>
        {
            entity.HasKey(mr => mr.Id);

            entity.HasOne(mr => mr.Match)
                .WithMany(m => m.Requests)
                .HasForeignKey(mr => mr.MatchId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(mr => mr.User)
                .WithMany(u => u.MatchRequests)
                .HasForeignKey(mr => mr.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Payment
        builder.Entity<Payment>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.HasOne(p => p.Booking)
                .WithOne(b => b.Payment)
                .HasForeignKey<Payment>(p => p.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.User)
                .WithMany(u => u.Payments)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(p => p.Amount)
                .HasColumnType("decimal(18,2)");
        });

        // Review
        builder.Entity<Review>(entity =>
        {
            entity.HasKey(r => r.Id);

            entity.HasOne(r => r.Stadium)
                .WithMany(s => s.Reviews)
                .HasForeignKey(r => r.StadiumId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(r => r.Rating)
                .HasColumnType("int")
                .HasAnnotation("Range", new[] { 1, 5 });
        });

        // Stadium
        builder.Entity<Stadium>(entity =>
        {
            entity.HasKey(s => s.Id);

            entity.HasOne(s => s.Owner)
                .WithMany(u => u.OwnedStadiums)
                .HasForeignKey(s => s.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(s => s.PricePerHour)
                .HasColumnType("decimal(18,2)");
        });

        // StadiumPhoto
        builder.Entity<StadiumPhoto>(entity =>
        {
            entity.HasKey(sp => sp.Id);

            entity.HasOne(sp => sp.Stadium)
                .WithMany(s => s.Photos)
                .HasForeignKey(sp => sp.StadiumId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ApplicationUser - Foreign Key OwnerId в Stadium это string
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.FullName)
                .IsRequired()
                .HasMaxLength(100);
        });
    }
}