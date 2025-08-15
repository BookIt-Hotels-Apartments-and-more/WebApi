namespace BookIt.API.Models.Responses;

public record ReviewResponse
{
    public int Id { get; set; }
    public string Text { get; set; } = null!;
    public ReviewerResponse? Author { get; set; }

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

    public DateTime? CreatedAt { get; set; }

    public bool IsCustomerReview => CustomerId.HasValue;
    public int? CustomerId { get; set; }

    public bool IsApartmentReview => ApartmentId.HasValue;
    public int? ApartmentId { get; set; }

    public BookingResponse Booking { get; set; } = null!;
    public List<ImageResponse> Photos { get; set; } = new();
}

public record ReviewerResponse
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public List<string> Photos { get; set; } = new();
}