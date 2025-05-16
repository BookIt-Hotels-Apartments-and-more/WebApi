using Microsoft.EntityFrameworkCore;
using BookIt.Entities;

namespace BookIt.Database;

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Establishment>()
            .HasOne(e => e.Owner)
            .WithMany(u => u.OwnedEstablishments)
            .HasForeignKey(e => e.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
