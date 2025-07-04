namespace BookIt.DAL.Models;

[Flags]
public enum ApartmentFeatures
{
    None = 0,
    FreeWifi = 1 << 0,
    AirConditioning = 1 << 1,
    Breakfast = 1 << 2,
    Kitchen = 1 << 3,
    TV = 1 << 4,
    Balcony = 1 << 5,
    Bathroom = 1 << 6,
    PetsAllowed = 1 << 7,

}
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

    public int EstablishmentId { get; set; }
    public Establishment Establishment { get; set; } = null!;
    
    public ICollection<Image> Photos { get; set; } = new List<Image>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}