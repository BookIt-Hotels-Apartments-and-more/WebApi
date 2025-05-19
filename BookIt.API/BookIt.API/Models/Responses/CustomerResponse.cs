namespace BookIt.API.Models.Responses;

public record CustomerResponse
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Phone { get; set; } = null!;
    public string? Bio { get; set; } = null!;
    public List<string> Photos { get; set; } = new();
}
