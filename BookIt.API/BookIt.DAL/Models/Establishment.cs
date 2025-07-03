namespace BookIt.DAL.Models;


public enum EstablishmentType
{
    Hotel,
    Hostel,
    Villa,
    Apartment,
    Cottage,
}

[Flags]
public enum EstablishmentFeatures
{
    None = 0,
    Parking = 1 << 0,
    Pool = 1 << 1,
    Beach = 1 << 2,
    Fishing = 1 << 3,
    Sauna = 1 << 4,
    Restaurant = 1 << 5,
    Smoking = 1 << 6,
    AccessibleForDisabled = 1 << 7,
    ElectricCarCharging = 1 << 8,
    Elevator = 1 << 9,
}

public class Establishment
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string Description { get; set; } = null!;
    public EstablishmentType Type { get; set; }
    public EstablishmentFeatures Features { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public TimeOnly CheckInTime { get; set; } = new TimeOnly(14, 0);
    public TimeOnly CheckOutTime { get; set; } = new TimeOnly(12, 0);

    public int OwnerId { get; set; }
    public User Owner { get; set; } = null!;

    public ICollection<Image> Photos { get; set; } = new List<Image>();
    public ICollection<Apartment> Apartments { get; set; } = new List<Apartment>();
}