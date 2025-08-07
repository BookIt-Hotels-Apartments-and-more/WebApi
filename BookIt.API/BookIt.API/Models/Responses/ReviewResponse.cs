namespace BookIt.API.Models.Responses;

public record ReviewResponse
{
    public int Id { get; set; }
    public string Text { get; set; } = null!;

    public float Rating { get; set; }

    // Apartment review ratings (nullable - only present for apartment reviews)
    public float? StaffRating { get; set; }
    public float? PurityRating { get; set; }
    public float? PriceQualityRating { get; set; }
    public float? ComfortRating { get; set; }
    public float? FacilitiesRating { get; set; }
    public float? LocationRating { get; set; }

    // User review rating (nullable - only present for user reviews)
    public float? CustomerStayRating { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int BookingId { get; set; }
    public BookingResponse Booking { get; set; } = null!;
    public List<ImageResponse> Photos { get; set; } = new();
}