namespace BookIt.BLL.DTOs;

public record BookingDTO
{
    public int Id { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public bool IsCheckedIn { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? AdditionalRequests { get; set; }
    public int CustomerId { get; set; }
    public CustomerDTO Customer { get; set; } = null!;
    public int ApartmentId { get; set; }
    public ApartmentDTO Apartment { get; set; } = null!;

    public bool? HasCustomerReviewed { get; set; } = null;
    public bool? HasLandlordReviewed { get; set; } = null;
}
