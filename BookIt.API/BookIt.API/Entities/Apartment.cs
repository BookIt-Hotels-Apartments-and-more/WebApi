namespace BookIt.Entities;

public class Apartment
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public double Price { get; set; }
    public int Capacity { get; set; }
    public double Rating { get; set; }
    public string Description { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int EstablishmentId { get; set; }
    public Establishment Establishment { get; set; } = null!;
    public ICollection<Image> Photos { get; set; } = new List<Image>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}