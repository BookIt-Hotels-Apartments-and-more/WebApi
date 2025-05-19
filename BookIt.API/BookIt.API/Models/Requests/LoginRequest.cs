namespace BookIt.API.Models.Requests;

public record LoginRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}
