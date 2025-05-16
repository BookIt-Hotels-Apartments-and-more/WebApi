namespace BookIt.Entities;

public class Review
{
    public int Id { get; set; }
    public string Text { get; set; } = null!;
    public double Rating { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;
    public ICollection<Image> Photos { get; set; } = new List<Image>();
}
