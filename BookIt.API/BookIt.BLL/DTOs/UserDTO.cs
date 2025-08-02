using BookIt.DAL.Enums;

namespace BookIt.BLL.DTOs;

public record UserDTO
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

    public RatingDTO? Rating { get; set; }
    public ICollection<ImageDTO> Photos { get; set; } = new List<ImageDTO>();
    public ICollection<ReviewDTO> Reviews { get; set; } = new List<ReviewDTO>();
    public ICollection<BookingDTO> Bookings { get; set; } = new List<BookingDTO>();
    public ICollection<FavoriteDTO> Favorites { get; set; } = new List<FavoriteDTO>();
    public ICollection<EstablishmentDTO> OwnedEstablishments { get; set; } = new List<EstablishmentDTO>();
}
