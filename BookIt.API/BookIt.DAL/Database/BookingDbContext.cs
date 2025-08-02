using Microsoft.EntityFrameworkCore;
using BookIt.DAL.Models;

namespace BookIt.DAL.Database;

public class BookingDbContext : DbContext
{
    public BookingDbContext(DbContextOptions<BookingDbContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Establishment> Establishments { get; set; }
    public DbSet<Apartment> Apartments { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Image> Images { get; set; }
    public DbSet<Favorite> Favorites { get; set; }
    public DbSet<Geolocation> Geolocations { get; set; }
    public DbSet<Rating> Ratings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Establishment>()
            .HasOne(e => e.Owner)
            .WithMany(u => u.OwnedEstablishments)
            .HasForeignKey(e => e.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Review>()
            .HasOne(a => a.Apartment)
            .WithMany(a => a.Reviews)
            .HasForeignKey(a => a.ApartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Review>()
            .HasOne(a => a.User)
            .WithMany(a => a.Reviews)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Apartment>()
            .HasOne(a => a.Rating)
            .WithMany(r => r.Apartments)
            .HasForeignKey(a => a.RatingId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Establishment>()
            .HasOne(e => e.Rating)
            .WithMany(r => r.Establishments)
            .HasForeignKey(e => e.RatingId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<User>()
            .HasOne(u => u.Rating)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RatingId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}