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
    public DbSet<ApartmentRating> ApartmentRatings { get; set; }
    public DbSet<UserRating> UserRatings { get; set; }

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
            .HasOne(a => a.ApartmentRating)
            .WithMany(r => r.Apartments)
            .HasForeignKey(a => a.ApartmentRatingId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Establishment>()
            .HasOne(e => e.ApartmentRating)
            .WithMany(r => r.Establishments)
            .HasForeignKey(e => e.ApartmentRatingId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<User>()
            .HasOne(u => u.UserRating)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.UserRatingId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.PhoneNumber)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<Payment>()
            .Property(p => p.Amount)
            .HasPrecision(18, 2);

        var bookingIdIndexOnReviews = modelBuilder.Entity<Review>().Metadata
            .GetIndexes().FirstOrDefault(i => i.GetDatabaseName() == "IX_Reviews_BookingId");
        
        if (bookingIdIndexOnReviews is not null)
            modelBuilder.Entity<Review>().Metadata.RemoveIndex(bookingIdIndexOnReviews);
    }
}