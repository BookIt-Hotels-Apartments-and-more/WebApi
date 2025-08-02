using BookIt.DAL.Enums;

namespace BookIt.DAL.Models;

public class Apartment
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public double Price { get; set; }
    public int Capacity { get; set; }
    public double Area { get; set; }
    public string Description { get; set; } = null!;
    public ApartmentFeatures Features { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? RatingId { get; set; }
    public Rating? Rating { get; set; }

    public int EstablishmentId { get; set; }
    public Establishment Establishment { get; set; } = null!;

    public ICollection<Image> Photos { get; set; } = new List<Image>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}