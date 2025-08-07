namespace BookIt.API.Models.Responses;

public record OwnerResponse
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Phone { get; set; } = null!;
    public string? Bio { get; set; } = null!;
    public UserRatingResponse? Rating { get; set; }
    public List<string> Photos { get; set; } = new();
}