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
}
