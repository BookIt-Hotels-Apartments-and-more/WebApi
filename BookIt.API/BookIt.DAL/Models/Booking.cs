namespace BookIt.DAL.Models;

public class Booking
{
    public int Id { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public bool IsCheckedIn { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int ApartmentId { get; set; }
    public Apartment Apartment { get; set; } = null!;

    public Review? Review { get; set; } = null!;

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}