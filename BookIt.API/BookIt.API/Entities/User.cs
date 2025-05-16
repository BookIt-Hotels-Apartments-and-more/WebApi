namespace BookIt.Entities;

public enum UserRole
{
    Admin,
    Landlord,
    Tenant
}

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string? Bio { get; set; }
    public UserRole Role { get; set; }
    public double? Rating { get; set; } = null;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastActiveAt { get; set; }
    public ICollection<Image> Photos { get; set; } = new List<Image>();
    public ICollection<Establishment> OwnedEstablishments { get; set; } = new List<Establishment>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
}