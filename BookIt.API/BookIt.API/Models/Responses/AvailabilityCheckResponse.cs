namespace BookIt.API.Models.Responses;

public record AvailabilityCheckResponse
{
    public int ApartmentId { get; set; }
    public bool IsAvailable { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
}