namespace BookIt.BLL.DTOs;

public record UserDetailsDTO
{
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string? Bio { get; set; } = null;
}
