namespace BookIt.API.Models.Requests;

public record BookingRequest
{
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public int CustomerId { get; set; }
    public int ApartmentId { get; set; }
    public string? AdditionalRequests { get; set; }
}
