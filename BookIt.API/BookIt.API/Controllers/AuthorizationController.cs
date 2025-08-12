using BookIt.API.Models.Requests;
using BookIt.BLL.Interfaces;
using BookIt.BLL.Models.Responses;
using BookIt.BLL.Services;
using BookIt.DAL.Configuration.Settings;
using BookIt.DAL.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BookIt.API.Controllers;

[ApiController]
[Route("auth")]
public class AuthorizationController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IJWTService _jwtService;
    private readonly IEmailSenderService _emailSenderService;
    private readonly IOptions<AppSettings> _appSettingsOptions;
    private readonly IOptions<UrlSettings> _urlSettingsOptions;

    public AuthorizationController(
        IUserService userService,
        IJWTService jwtService,
        IEmailSenderService emailSenderService,
        IOptions<AppSettings> appSettingsOptions,
        IOptions<UrlSettings> urlSettingsOptions
    )
    {
        _userService = userService;
        _jwtService = jwtService;
        _emailSenderService = emailSenderService;
        _appSettingsOptions = appSettingsOptions;
        _urlSettingsOptions = urlSettingsOptions;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var baseUrl = _appSettingsOptions.Value.BaseUrl;

        var user = await _userService.RegisterAsync(request.Username, request.Email, request.Password, UserRole.Tenant);
        var confirmationLink = $"{baseUrl}/auth/verify-email?token={user.EmailConfirmationToken}";
        var body = $"Please confirm your email by clicking the following link: {confirmationLink}";

        _emailSenderService.SendEmail(user.Email, "Email Confirmation", body);
        return Ok(new { user.Id, user.Username, user.Email, user.Role, user.CreatedAt });
    }

    [HttpPost("register-landlord")]
    public async Task<IActionResult> RegisterLandlord([FromBody] RegisterRequest request)
    {
        var baseUrl = _appSettingsOptions.Value.BaseUrl;

        var user = await _userService.RegisterAsync(request.Username, request.Email, request.Password, UserRole.Landlord);
        var confirmationLink = $"{baseUrl}/auth/verify-email?token={user.EmailConfirmationToken}";
        var body = $"Please confirm your email by clicking the following link: {confirmationLink}";

        _emailSenderService.SendEmail(user.Email, "Email Confirmation", body);
        return Ok(new { user.Id, user.Username, user.Email, user.Role, user.CreatedAt });
    }

    [HttpPost("register-admin")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RegisterAdmin([FromBody] RegisterRequest request)
    {
        var baseUrl = _appSettingsOptions.Value.BaseUrl;

        var user = await _userService.RegisterAsync(request.Username, request.Email, request.Password, UserRole.Admin);
        var confirmationLink = $"{baseUrl}/auth/verify-email?token={user.EmailConfirmationToken}";
        var body = $"Please confirm your email by clicking the following link: {confirmationLink}";

        _emailSenderService.SendEmail(user.Email, "Email Confirmation", body);
        return Ok(new { user.Id, user.Username, user.Email, user.Role, user.CreatedAt });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var user = await _userService.ResetPasswordAsync(request.Token, request.NewPassword);
        return Ok();
    }

    [HttpPost("reset-password/generate-token")]
    public async Task<IActionResult> ResetPasswordToken([FromBody] GenerateResetPasswordTokenRequest request)
    {
        var baseUrl = _appSettingsOptions.Value.BaseUrl;

        var user = await _userService.GenerateResetPasswordTokenAsync(request.Email);
        var confirmationLink = $"{baseUrl}/auth/reset-token?token={user.ResetPasswordToken}";
        var body = $"Password Reset: {confirmationLink}";

        _emailSenderService.SendEmail(user.Email, "Password Reset", body);
        return Ok();
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
            Token = token,
            Role = (int)user.Role,
        });
    }

    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmail()
    {
        var clientUrl = _urlSettingsOptions.Value.ClientUrl;
        try
        {
            var token = HttpContext.Request.Query["token"];
            await _userService.VerifyEmailAsync(token);
            return Redirect($"{clientUrl}/email-confirmed");
        }
        catch
        {
            return Redirect($"{clientUrl}/email-not-confirm");
        }

    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        int userId = int.Parse(userIdStr);

        var user = await _userService.GetUserByIdAsync(userId);

        if (user is null) return Unauthorized();

        return Ok(new UserAuthResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = (int)user.Role,
        });
    }
}
