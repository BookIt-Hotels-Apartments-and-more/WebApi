namespace BookIt.API.Models.Responses;

public class UserAuthResponse
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public int Role { get; set; }
    public string? Token { get; set; } = null;
}