using BookIt.DAL.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookIt.DAL.Models;

public class Establishment
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public EstablishmentType Type { get; set; }
    public EstablishmentFeatures Features { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public TimeOnly CheckInTime { get; set; } = new TimeOnly(14, 0);
    public TimeOnly CheckOutTime { get; set; } = new TimeOnly(12, 0);

    public VibeType? Vibe { get; set; } = VibeType.None;

    public int? ApartmentRatingId { get; set; }
    public ApartmentRating? ApartmentRating { get; set; }

    public int OwnerId { get; set; }
    public User Owner { get; set; } = null!;

    public int? GeolocationId { get; set; }
    public Geolocation? Geolocation { get; set; } = null!;

    public ICollection<Image> Photos { get; set; } = new List<Image>();
    public ICollection<Apartment> Apartments { get; set; } = new List<Apartment>();

    [NotMapped]
    public double? MinApartmentPrice { get; set; }
    [NotMapped]
    public double? MaxApartmentPrice { get; set; }
}