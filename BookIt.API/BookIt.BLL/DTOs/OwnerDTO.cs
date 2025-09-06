namespace BookIt.BLL.DTOs;

public record OwnerDTO
{
    public int? Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; } = null!;
    public string? Bio { get; set; } = null!;
    public bool IsRestricted { get; set; }
    public List<string> Photos { get; set; } = new();
}