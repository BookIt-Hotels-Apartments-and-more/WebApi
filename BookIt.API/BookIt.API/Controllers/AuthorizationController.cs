using BookIt.BLL.Services;
using BookIt.DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BookIt.API.Models.Requests;
using BookIt.BLL.Models.Responses;
using BookIt.BLL.Interfaces;

namespace BookIt.API.Controllers;

[ApiController]
[Route("auth")]
public class AuthorizationController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IJWTService _jwtService;
    private readonly IEmailSenderService _emailSenderService;
    private readonly IConfiguration _configuration;

    public AuthorizationController(
        IUserService userService,
        IJWTService jwtService,
        IEmailSenderService emailSenderService,
        IConfiguration configuration
    )
    {
        _userService = userService;
        _jwtService = jwtService;
        _emailSenderService = emailSenderService;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var baseUrl = _configuration.GetValue<string>("AppSettings:BaseUrl");

        try
        {
            var user = await _userService.RegisterAsync(request.Username, request.Email, request.Password, UserRole.Tenant);
            var confirmationLink = $"{baseUrl}/auth/verify-email?token={user.EmailConfirmationToken}";
            var body = $"Please confirm your email by clicking the following link: {confirmationLink}";

            _emailSenderService.SendEmail(user.Email, "Email Confirmation", body);
            return Ok(new { user.Id, user.Username, user.Email, user.Role, user.CreatedAt });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        try
        {
            var user = await _userService.ResetPasswordAsync(request.Token, request.NewPassword);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("reset-password/generate-token")]
    public async Task<IActionResult> ResetPasswordToken([FromBody] GenerateResetPasswordTokenRequest request)
    {
        var baseUrl = _configuration.GetValue<string>("AppSettings:BaseUrl");

        try
        {
            var user = await _userService.GenerateResetPasswordTokenAsync(request.Email);
            var confirmationLink = $"{baseUrl}/auth/reset-token?token={user.ResetPasswordToken}";
            var body = $"Password Reset: {confirmationLink}";

            _emailSenderService.SendEmail(user.Email, "Password Reset", body);
            return Ok();
        }
        catch (Exception)
        {
            return BadRequest();
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userService.LoginAsync(request.Email, request.Password);

        if (user is null)
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        var token = _jwtService.GenerateToken(user);

        return Ok(new UserAuthResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Token = token
        });
    }

    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmail()
    {
        string token = HttpContext.Request.Query["token"];

        try
        {
            await _userService.VerifyEmailAsync(token);
            return Redirect("https://localhost:5173/email-confirmed");
        }
        catch
        {
            return Redirect("https://localhost:5173/email-not-confirm");
        }

    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        int userId = int.Parse(userIdStr);

        var _user = await _userService.GetUserByIdAsync(userId);

        if (_user is null)
        {
            return Unauthorized();
        }

        return Ok(new UserAuthResponse
        {
            Id = _user.Id,
            Username = _user.Username,
            Email = _user.Email,
        });
    }
}
