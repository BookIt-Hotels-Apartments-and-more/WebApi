using BookIt.DAL.Models;

namespace BookIt.BLL.DTOs;

public record ReviewDTO
{
    public int Id { get; set; }
    public string Text { get; set; } = null!;
    public float Rating { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int BookingId { get; set; }
    public BookingDTO Booking { get; set; } = null!;
    public List<string> Photos { get; set; } = new();
}
