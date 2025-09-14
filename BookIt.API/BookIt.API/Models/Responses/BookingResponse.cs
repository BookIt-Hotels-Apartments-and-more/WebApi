namespace BookIt.API.Models.Responses;

public record BookingResponse
{
    public int Id { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public bool IsCheckedIn { get; set; } = false;
    public DateTime? CreatedAt { get; set; }
    public string? AdditionalRequests { get; set; }
    public CustomerResponse Customer { get; set; } = null!;
    public ApartmentResponse Apartment { get; set; } = null!;

    public bool? IsPaid { get; set; } = null;

    public bool? HasCustomerReviewed { get; set; } = null;
    public bool? HasLandlordReviewed { get; set; } = null;
}
