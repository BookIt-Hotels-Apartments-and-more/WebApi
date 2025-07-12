using BookIt.DAL.Enums;

namespace BookIt.DAL.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public bool IsEmailConfirmed { get; set; } = false;
    public string? EmailConfirmationToken { get; set; } = null;
    public string? ResetPasswordToken { get; set; } = null;
    public string? PasswordHash { get; set; } = null;
    public string? PhoneNumber { get; set; }
    public string? Bio { get; set; }
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastActiveAt { get; set; }

    public ICollection<Image> Photos { get; set; } = new List<Image>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public ICollection<Establishment> OwnedEstablishments { get; set; } = new List<Establishment>();
}