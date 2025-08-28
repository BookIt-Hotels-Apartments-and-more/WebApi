namespace BookIt.BLL.DTOs;

public record OwnerDTO
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Phone { get; set; } = null!;
    public string? Bio { get; set; } = null!;
    public bool IsRestricted { get; set; }
    public List<string> Photos { get; set; } = new();
}