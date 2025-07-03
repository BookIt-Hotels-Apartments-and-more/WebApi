namespace BookIt.API.Models.Requests;

public record ReviewRequest
{
    public string Text { get; set; } = null!;
    public float Rating { get; set; }
    public int BookingId { get; set; }
    public int? CustomerId { get; set; }
    public int? ApartmentId { get; set; }
    public List<int> ExistingPhotosIds { get; set; } = new();
    public List<string> NewPhotosBase64 { get; set; } = new();
}
