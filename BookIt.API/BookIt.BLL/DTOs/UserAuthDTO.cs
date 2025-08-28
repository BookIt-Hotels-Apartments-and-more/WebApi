using BookIt.DAL.Enums;

namespace BookIt.BLL.DTOs;

public record UserAuthDTO
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public UserRole Role { get; set; }
    public string Token { get; set; } = null!;
    public string? EmailConfirmationToken { get; set; } = null;
    public bool IsRestricted { get; set; }
}
