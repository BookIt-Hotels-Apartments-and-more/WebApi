using BookIt.DAL.Constants;

namespace BookIt.DAL.Models;

public class Review
{
    public int Id { get; set; }
    public string Text { get; set; } = null!;
    public float Rating { get; set; }

    public float StaffRating { get; set; }
    public float PurityRating { get; set; }
    public float PriceQualityRating { get; set; }
    public float ComfortRating { get; set; }
    public float FacilitiesRating { get; set; }
    public float LocationRating { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? UserId { get; set; }
    public User? User { get; set; } = null!;

    public int? ApartmentId { get; set; }
    public Apartment? Apartment { get; set; } = null!;

    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;

    public ICollection<Image> Photos { get; set; } = new List<Image>();

    public void UpdateOverallRating()
    {
        var ratings = new[] { StaffRating, PurityRating, PriceQualityRating, ComfortRating, FacilitiesRating, LocationRating };
        Rating = ratings.Average();
    }
}