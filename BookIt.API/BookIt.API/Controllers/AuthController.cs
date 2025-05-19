using BookIt.BLL.Services;
using BookIt.DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BookIt.API.Models.Requests;

namespace BookIt.API.Controllers;

[ApiController]
[Route("auth")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IJWTService _jwtService;

    public UsersController(IUserService userService, IJWTService jwtService)
    {
        _userService = userService;
        _jwtService = jwtService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var user = await _userService.RegisterAsync(request.Username, request.Email, request.Password, UserRole.Tenant);
            return Ok(new { user.Id, user.Username, user.Email, user.Role, user.CreatedAt });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userService.LoginAsync(request.Email, request.Password);
        if (user == null)
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        var token = _jwtService.GenerateToken(user);
        return Ok(new
        {
            user.Id,
            user.Username,
            user.Email,
            user.Role,
            Token = token
        });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        int userId = Int32.Parse(userIdStr);

        var _user = await _userService.GetUserByIdAsync(userId);

        _user.PasswordHash = null; // безопастность - самое главное (потом исправлю этот костыль)

        return Ok(new
        {
            user = _user
        });
    }
}
