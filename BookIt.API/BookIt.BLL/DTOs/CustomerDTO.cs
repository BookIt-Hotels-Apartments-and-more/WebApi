namespace BookIt.BLL.DTOs;

public record CustomerDTO
{
    public int? Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; } = null!;
    public string? Bio { get; set; } = null!;
    public int? RatingId { get; set; }
    public bool IsRestricted { get; set; }
    public UserRatingDTO? Rating { get; set; }
    public List<string> Photos { get; set; } = new();
}
