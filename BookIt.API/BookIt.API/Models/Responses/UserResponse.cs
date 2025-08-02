using BookIt.DAL.Enums;

namespace BookIt.API.Models.Responses;

public record UserResponse
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public bool IsEmailConfirmed { get; set; } = false;
    public string? PhoneNumber { get; set; }
    public string? Bio { get; set; }
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastActiveAt { get; set; }

    public RatingResponse? Rating { get; set; }
    public ICollection<ImageResponse> Photos { get; set; } = new List<ImageResponse>();
    public ICollection<ReviewResponse> Reviews { get; set; } = new List<ReviewResponse>();
    public ICollection<BookingResponse> Bookings { get; set; } = new List<BookingResponse>();
    public ICollection<FavoriteResponse> Favorites { get; set; } = new List<FavoriteResponse>();
    public ICollection<EstablishmentResponse> OwnedEstablishments { get; set; } = new List<EstablishmentResponse>();
}
