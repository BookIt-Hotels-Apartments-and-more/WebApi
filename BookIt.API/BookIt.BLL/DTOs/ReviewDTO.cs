namespace BookIt.BLL.DTOs;

public record ReviewDTO
{
    public int Id { get; set; }
    public string Text { get; set; } = null!;
    public float Rating { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? CustomerId { get; set; }
    public CustomerDTO? Customer { get; set; } = null!;
    public int? ApartmentId { get; set; }
    public ApartmentDTO? Apartment { get; set; } = null!;
    public int BookingId { get; set; }
    public BookingDTO Booking { get; set; } = null!;
    public List<ImageDTO> Photos { get; set; } = new();
}
