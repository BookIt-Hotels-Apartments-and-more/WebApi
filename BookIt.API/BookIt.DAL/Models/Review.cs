namespace BookIt.DAL.Models;

public class Review
{
    public int Id { get; set; }
    public string Text { get; set; } = null!;

    public float Rating { get; set; }

    // Apartment review ratings (nullable - only set for apartment reviews)
    public float? StaffRating { get; set; }
    public float? PurityRating { get; set; }
    public float? PriceQualityRating { get; set; }
    public float? ComfortRating { get; set; }
    public float? FacilitiesRating { get; set; }
    public float? LocationRating { get; set; }

    // User review rating (nullable - only set for user reviews)
    public float? CustomerStayRating { get; set; }

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
        if (ApartmentId.HasValue && HasApartmentRatings())
        {
            var apartmentRatings = new[] { StaffRating!.Value, PurityRating!.Value, PriceQualityRating!.Value,
                                         ComfortRating!.Value, FacilitiesRating!.Value, LocationRating!.Value };
            Rating = apartmentRatings.Average();
        }
        else if (UserId.HasValue && CustomerStayRating.HasValue)
        {
            Rating = CustomerStayRating.Value;
        }
    }

    private bool HasApartmentRatings()
    {
        return StaffRating.HasValue && PurityRating.HasValue && PriceQualityRating.HasValue &&
               ComfortRating.HasValue && FacilitiesRating.HasValue && LocationRating.HasValue;
    }
}