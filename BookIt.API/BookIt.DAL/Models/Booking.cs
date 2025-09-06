namespace BookIt.DAL.Models;

public class Booking
{
    public int Id { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public bool IsCheckedIn { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? AdditionalRequests { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int ApartmentId { get; set; }
    public Apartment Apartment { get; set; } = null!;

    public ICollection<Review> Reviews { get; set; } = new List<Review>();

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}