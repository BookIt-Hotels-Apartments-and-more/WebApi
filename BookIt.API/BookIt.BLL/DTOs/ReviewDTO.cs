namespace BookIt.BLL.DTOs;

public record ReviewDTO
{
    public int Id { get; set; }
    public string Text { get; set; } = null!;

    public float Rating { get; set; }

    public float? StaffRating { get; set; }
    public float? PurityRating { get; set; }
    public float? PriceQualityRating { get; set; }
    public float? ComfortRating { get; set; }
    public float? FacilitiesRating { get; set; }
    public float? LocationRating { get; set; }

    public float? CustomerStayRating { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool IsCustomerReview => CustomerId.HasValue;
    public int? CustomerId { get; set; }
    public CustomerDTO? Customer { get; set; }

    public bool IsApartmentReview => ApartmentId.HasValue;
    public int? ApartmentId { get; set; }
    public ApartmentDTO? Apartment { get; set; }

    public int BookingId { get; set; }
    public BookingDTO Booking { get; set; } = null!;

    public List<ImageDTO> Photos { get; set; } = new();
}
