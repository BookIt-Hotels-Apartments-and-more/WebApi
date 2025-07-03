namespace BookIt.DAL.Models;

[Flags]
public enum ApartmentFeatures
{
    None = 0,
    FreeWifi = 1,
    AirConditioning = 2,
    BreakfastIncluded = 4,
    Parking = 8,
    Kitchen = 16,
    Pool = 32,
}

public class Apartment
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public double Price { get; set; }
    public int Capacity { get; set; } // місткість
    public string Description { get; set; } = null!;
    public ApartmentFeatures Features { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int EstablishmentId { get; set; }
    public Establishment Establishment { get; set; } = null!;


    public ICollection<Image> Photos { get; set; } = new List<Image>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}