using AutoMapper;
using BookIt.API.Models.Requests;
using BookIt.API.Models.Responses;
using BookIt.BLL.Interfaces;
using BookIt.DAL.Configuration.Settings;
using BookIt.DAL.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace BookIt.API.Controllers;

[ApiController]
[Route("auth")]
public class AuthorizationController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IJWTService _jwtService;
    private readonly IUserService _userService;
    private readonly IEmailSenderService _emailSenderService;
    private readonly IOptions<AppSettings> _appSettingsOptions;
    private readonly IOptions<UrlSettings> _urlSettingsOptions;

    public AuthorizationController(
        IMapper mapper,
        IJWTService jwtService,
        IUserService userService,
        IEmailSenderService emailSenderService,
        IOptions<AppSettings> appSettingsOptions,
        IOptions<UrlSettings> urlSettingsOptions)
    {
        _mapper = mapper;
        _jwtService = jwtService;
        _userService = userService;
        _emailSenderService = emailSenderService;
        _appSettingsOptions = appSettingsOptions;
        _urlSettingsOptions = urlSettingsOptions;
    }

    [HttpPost("register")]
    [ResponseCache(NoStore = true)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var baseUrl = _appSettingsOptions.Value.BaseUrl;

        var user = await _userService.RegisterAsync(request.Username, request.Email, request.Password);
        var confirmationLink = $"{baseUrl}/auth/verify-email?token={user.EmailConfirmationToken}";
        var body = $"Please confirm your email by clicking the following link: {confirmationLink}";
        _emailSenderService.SendEmail(user.Email, "Email Confirmation", body);
        var response = _mapper.Map<UserAuthResponse>(user);
        Response.Headers.Append("Content-Encoding", "identity");
        return Ok(response);
    }

    [HttpPost("register-landlord")]
    [ResponseCache(NoStore = true)]
    public async Task<IActionResult> RegisterLandlord([FromBody] RegisterRequest request)
    {
        var baseUrl = _appSettingsOptions.Value.BaseUrl;

        var user = await _userService.RegisterAsync(request.Username, request.Email, request.Password, UserRole.Landlord);
        var confirmationLink = $"{baseUrl}/auth/verify-email?token={user.EmailConfirmationToken}";
        var body = $"Please confirm your email by clicking the following link: {confirmationLink}";
        _emailSenderService.SendEmail(user.Email, "Email Confirmation", body);
        var response = _mapper.Map<UserAuthResponse>(user);
        Response.Headers.Append("Content-Encoding", "identity");
        return Ok(response);
    }

    [HttpPost("register-admin")]
    [Authorize(Roles = "Admin")]
    [ResponseCache(NoStore = true)]
    public async Task<IActionResult> RegisterAdmin([FromBody] RegisterRequest request)
    {
        var baseUrl = _appSettingsOptions.Value.BaseUrl;

        var user = await _userService.RegisterAsync(request.Username, request.Email, request.Password, UserRole.Admin);
        var confirmationLink = $"{baseUrl}/auth/verify-email?token={user.EmailConfirmationToken}";
        var body = $"Please confirm your email by clicking the following link: {confirmationLink}";
        _emailSenderService.SendEmail(user.Email, "Email Confirmation", body);
        var response = _mapper.Map<UserAuthResponse>(user);
        Response.Headers.Append("Content-Encoding", "identity");
        return Ok(response);
    }

    [HttpPost("login")]
    [ResponseCache(NoStore = true)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userService.LoginAsync(request.Email, request.Password);
        var response = _mapper.Map<UserAuthResponse>(user);
        response.Token = await _jwtService.GenerateToken(user);
        Response.Headers.Append("Content-Encoding", "identity");
        return Ok(response);
    }

    [HttpPost("reset-password/generate-token")]
    [ResponseCache(NoStore = true)]
    public async Task<IActionResult> ResetPasswordToken([FromBody] GenerateResetPasswordTokenRequest request)
    {
        var clientUrl = _urlSettingsOptions.Value.ClientUrl;

        var user = await _userService.GenerateResetPasswordTokenAsync(request.Email);
        var confirmationLink = $"{clientUrl}/auth/reset-password?token={user!.ResetPasswordToken}";
        var body = $"Go to this link to reset your password: {confirmationLink}";
        _emailSenderService.SendEmail(user.Email, "Password Reset", body);
        Response.Headers.Append("Content-Encoding", "identity");
        return Ok();
    }

    [HttpPost("reset-password")]
    [ResponseCache(NoStore = true)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var user = await _userService.ResetPasswordAsync(request.Token, request.NewPassword);
        Response.Headers.Append("Content-Encoding", "identity");
        return Ok();
    }

    [HttpGet("verify-email")]
    [ResponseCache(NoStore = true)]
    public async Task<IActionResult> VerifyEmail()
    {
        var clientUrl = _urlSettingsOptions.Value.ClientUrl;
        try
        {
            var token = HttpContext.Request.Query["token"].ToString();
            if (string.IsNullOrEmpty(token)) throw new Exception();
            await _userService.VerifyEmailAsync(token);
            return Redirect($"{clientUrl}/email-confirmed");
        }
        catch
        {
            return Redirect($"{clientUrl}/email-not-confirm");
        }
    }

    [HttpGet("me")]
    [Authorize(Roles = "Tenant,Landlord,Admin")]
    [ResponseCache(NoStore = true)]
    public async Task<IActionResult> Me()
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
        var user = await _userService.GetUserByIdAsync(int.Parse(userIdStr));
        var response = _mapper.Map<UserAuthResponse>(user);
        Response.Headers.Append("Content-Encoding", "identity");
        return Ok(response);
    }
}
