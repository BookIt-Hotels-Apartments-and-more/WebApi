using BookIt.DAL.Enums;

namespace BookIt.BLL.DTOs;

public record EstablishmentDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public EstablishmentType Type { get; set; }
    public string Description { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public TimeOnly CheckInTime { get; set; }
    public TimeOnly CheckOutTime { get; set; }
    public EstablishmentFeatures Features { get; set; } = new();
    public VibeType? Vibe { get; set; }
    public int OwnerId { get; set; }

    public int? RatingId { get; set; }
    public RatingDTO? Rating { get; set; }

    public OwnerDTO Owner { get; set; } = null!;
    public List<ImageDTO> Photos { get; set; } = new();
    public GeolocationDTO Geolocation { get; set; } = null!;
    public decimal Price { get; set; }
}