using BookIt.DAL.Constants;
using System.ComponentModel.DataAnnotations;

namespace BookIt.API.Models.Requests;

public record LoginRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(320, ErrorMessage = "Email cannot exceed 320 characters")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(UserConstants.MaxPasswordLength, MinimumLength = UserConstants.MinPasswordLength, ErrorMessage = "Password must be between 8 and 128 characters")]
    // no additional security validation for password in login request because it is not a registration or password change
    public string Password { get; set; } = null!;
}
